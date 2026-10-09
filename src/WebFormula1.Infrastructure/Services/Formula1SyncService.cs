using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebFormula1.Core.Models;
using WebFormula1.Infrastructure.Api;
using WebFormula1.Infrastructure.Data;

namespace WebFormula1.Infrastructure.Services;

public interface IFormula1SyncService
{
    Task SyncSeasonAsync(int year, CancellationToken ct);
    Task SyncScheduleAsync(int year, CancellationToken ct);
    Task SyncTeamsAndDriversAsync(int year, CancellationToken ct);
    Task SyncStandingsAsync(int year, CancellationToken ct);
    Task SyncResultsAsync(int year, CancellationToken ct);
}

public class Formula1SyncService : IFormula1SyncService
{
    private readonly IJolpicaClient _jolpica;
    private readonly IWikipediaClient _wikipedia;
    private readonly Formula1DbContext _db;
    private readonly ILogger<Formula1SyncService> _logger;

    public Formula1SyncService(
        IJolpicaClient jolpica, IWikipediaClient wikipedia, Formula1DbContext db, ILogger<Formula1SyncService> logger)
    {
        _jolpica = jolpica;
        _wikipedia = wikipedia;
        _db = db;
        _logger = logger;
    }

    public async Task SyncSeasonAsync(int year, CancellationToken ct)
    {
        _logger.LogInformation("Sincronizando temporada {Year}", year);
        await SyncScheduleAsync(year, ct);
        await SyncTeamsAndDriversAsync(year, ct);
        await SyncStandingsAsync(year, ct);
        await SyncResultsAsync(year, ct);
        _logger.LogInformation("Temporada {Year} sincronizada", year);
    }

    public async Task SyncScheduleAsync(int year, CancellationToken ct)
    {
        var schedule = await _jolpica.GetScheduleAsync(year, ct);
        var existing = await _db.Races.Where(r => r.SeasonYear == year).ToDictionaryAsync(r => r.Round, ct);

        foreach (var item in schedule)
        {
            var round = ToInt(item.Round);
            if (!existing.TryGetValue(round, out var race))
            {
                race = new Race { SeasonYear = year, Round = round };
                _db.Races.Add(race);
            }

            race.Name = item.RaceName;
            race.Url = item.Url;
            race.Circuit = item.Circuit.CircuitName;
            race.Locality = item.Circuit.Location.Locality;
            race.Country = item.Circuit.Location.Country;
            race.RaceStartUtc = ParseUtc(item.Date, item.Time);
            race.FirstPracticeUtc = ParseSession(item.FirstPractice);
            race.SecondPracticeUtc = ParseSession(item.SecondPractice);
            race.ThirdPracticeUtc = ParseSession(item.ThirdPractice);
            race.QualifyingUtc = ParseSession(item.Qualifying);
            race.SprintUtc = ParseSession(item.Sprint);
            race.SprintQualifyingUtc = ParseSession(item.SprintQualifying ?? item.SprintShootout);
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task SyncTeamsAndDriversAsync(int year, CancellationToken ct)
    {
        var teams = await _db.Teams.ToDictionaryAsync(t => t.ExternalId, ct);
        var drivers = await _db.Drivers.ToDictionaryAsync(d => d.ExternalId, ct);

        foreach (var c in await _jolpica.GetConstructorsAsync(year, ct)) UpsertTeam(teams, c);
        foreach (var d in await _jolpica.GetDriversAsync(year, ct)) UpsertDriver(drivers, d);
        await _db.SaveChangesAsync(ct);

        foreach (var team in teams.Values.Where(t => t.History is null && t.Url is not null))
        {
            try
            {
                team.History = await _wikipedia.GetSummaryAsync(team.Url!, ct);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                _logger.LogWarning(ex, "No se pudo obtener la historia de {Team}", team.Name);
            }
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task SyncStandingsAsync(int year, CancellationToken ct)
    {
        var teams = await _db.Teams.ToDictionaryAsync(t => t.ExternalId, ct);
        var drivers = await _db.Drivers.ToDictionaryAsync(d => d.ExternalId, ct);

        var driverList = await _jolpica.GetDriverStandingsAsync(year, ct);
        var teamList = await _jolpica.GetConstructorStandingsAsync(year, ct);

        if (driverList?.DriverStandings is { Count: > 0 } driverStandings)
        {
            await _db.DriverStandings.Where(s => s.SeasonYear == year).ExecuteDeleteAsync(ct);
            foreach (var s in driverStandings)
            {
                var driver = UpsertDriver(drivers, s.Driver);
                if (s.Constructors.LastOrDefault() is { } current) driver.Team = UpsertTeam(teams, current);

                _db.DriverStandings.Add(new DriverStanding
                {
                    SeasonYear = year,
                    Round = ToInt(driverList.Round),
                    Driver = driver,
                    Position = ToInt(s.Position),
                    Points = ToDouble(s.Points),
                    Wins = ToInt(s.Wins)
                });
            }
        }

        if (teamList?.ConstructorStandings is { Count: > 0 } teamStandings)
        {
            await _db.TeamStandings.Where(s => s.SeasonYear == year).ExecuteDeleteAsync(ct);
            foreach (var s in teamStandings)
            {
                _db.TeamStandings.Add(new TeamStanding
                {
                    SeasonYear = year,
                    Round = ToInt(teamList.Round),
                    Team = UpsertTeam(teams, s.Constructor),
                    Position = ToInt(s.Position),
                    Points = ToDouble(s.Points),
                    Wins = ToInt(s.Wins)
                });
            }
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task SyncResultsAsync(int year, CancellationToken ct)
    {
        var results = await _jolpica.GetResultsAsync(year, ct);
        if (results.Count == 0) return;

        var races = await _db.Races.Where(r => r.SeasonYear == year).ToDictionaryAsync(r => r.Round, ct);
        var teams = await _db.Teams.ToDictionaryAsync(t => t.ExternalId, ct);
        var drivers = await _db.Drivers.ToDictionaryAsync(d => d.ExternalId, ct);

        var raceIds = races.Values.Select(r => r.Id).ToList();
        await _db.DriverResults.Where(r => raceIds.Contains(r.RaceId)).ExecuteDeleteAsync(ct);

        foreach (var item in results)
        {
            if (!races.TryGetValue(ToInt(item.Round), out var race)) continue;

            foreach (var r in item.Results ?? new())
            {
                _db.DriverResults.Add(new DriverResult
                {
                    Race = race,
                    Driver = UpsertDriver(drivers, r.Driver),
                    Team = UpsertTeam(teams, r.Constructor),
                    Position = ToInt(r.Position),
                    PositionText = r.PositionText,
                    Grid = ToInt(r.Grid),
                    Laps = ToInt(r.Laps),
                    Points = ToDouble(r.Points),
                    TimeText = r.Time?.Time,
                    Status = r.Status,
                    FastestLapRank = int.TryParse(r.FastestLap?.Rank, out var rank) ? rank : null
                });
            }
        }

        await _db.SaveChangesAsync(ct);
    }

    private Team UpsertTeam(Dictionary<string, Team> teams, ErgastConstructor c)
    {
        if (!teams.TryGetValue(c.ConstructorId, out var team))
        {
            team = new Team { ExternalId = c.ConstructorId };
            teams[c.ConstructorId] = team;
            _db.Teams.Add(team);
        }

        team.Name = c.Name;
        team.Nationality = c.Nationality;
        team.Url = c.Url ?? team.Url;
        return team;
    }

    private Driver UpsertDriver(Dictionary<string, Driver> drivers, ErgastDriver d)
    {
        if (!drivers.TryGetValue(d.DriverId, out var driver))
        {
            driver = new Driver { ExternalId = d.DriverId };
            drivers[d.DriverId] = driver;
            _db.Drivers.Add(driver);
        }

        driver.Code = d.Code ?? string.Empty;
        driver.GivenName = d.GivenName;
        driver.FamilyName = d.FamilyName;
        driver.Nationality = d.Nationality;
        driver.PermanentNumber = d.PermanentNumber;
        driver.Url = d.Url;
        driver.DateOfBirth = DateTime.TryParse(d.DateOfBirth, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dob) ? dob : null;
        return driver;
    }

    private static DateTime? ParseSession(ErgastSession? s) => s is null ? null : ParseUtc(s.Date, s.Time);

    private static DateTime ParseUtc(string date, string? time) =>
        DateTime.Parse($"{date}T{time ?? "00:00:00Z"}", CultureInfo.InvariantCulture,
            DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);

    private static int ToInt(string s) => int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : 0;

    private static double ToDouble(string s) => double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : 0;
}
