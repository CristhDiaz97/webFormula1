using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebFormula1.Core.Dtos;
using WebFormula1.Infrastructure.Data;

namespace WebFormula1.Api.Controllers;

[ApiController]
[Route("api/races")]
public class RacesController : ControllerBase
{
    private static readonly TimeSpan RaceDuration = TimeSpan.FromHours(3);
    private readonly Formula1DbContext _db;

    public RacesController(Formula1DbContext db) => _db = db;

    [HttpGet("seasons")]
    public async Task<List<int>> Seasons(CancellationToken ct) =>
        await _db.Races.Select(r => r.SeasonYear).Distinct().OrderByDescending(y => y).ToListAsync(ct);

    [HttpGet("next")]
    public async Task<ActionResult<RaceDto>> Next(CancellationToken ct)
    {
        var cutoff = DateTime.UtcNow - RaceDuration;
        var race = await _db.Races.Where(r => r.RaceStartUtc > cutoff).OrderBy(r => r.RaceStartUtc).FirstOrDefaultAsync(ct);
        return race is null ? NotFound() : ColombiaTime.ToDto(race, null);
    }

    [HttpGet("{year:int}")]
    public async Task<List<RaceDto>> BySeason(int year, CancellationToken ct)
    {
        var races = await _db.Races.Where(r => r.SeasonYear == year).OrderBy(r => r.Round).ToListAsync(ct);
        var winners = await _db.DriverResults
            .Where(r => r.Race.SeasonYear == year && r.Position == 1)
            .Select(r => new { r.RaceId, Name = r.Driver.GivenName + " " + r.Driver.FamilyName })
            .ToDictionaryAsync(x => x.RaceId, x => x.Name, ct);

        return races.Select(r => ColombiaTime.ToDto(r, winners.GetValueOrDefault(r.Id))).ToList();
    }

    [HttpGet("{year:int}/{round:int}")]
    public async Task<ActionResult<RaceDetailDto>> Detail(int year, int round, CancellationToken ct)
    {
        var race = await _db.Races.FirstOrDefaultAsync(r => r.SeasonYear == year && r.Round == round, ct);
        if (race is null) return NotFound();

        var results = await _db.DriverResults
            .Where(r => r.RaceId == race.Id)
            .OrderBy(r => r.Position)
            .Select(r => new RaceResultDto(
                r.Position, r.PositionText, r.Driver.GivenName + " " + r.Driver.FamilyName, r.Driver.Code,
                r.Team != null ? r.Team.Name : "", r.Grid, r.Laps, r.TimeText, r.Status, r.Points, r.FastestLapRank == 1))
            .ToListAsync(ct);

        return new RaceDetailDto(ColombiaTime.ToDto(race, results.FirstOrDefault(r => r.Position == 1)?.Driver), results);
    }
}
