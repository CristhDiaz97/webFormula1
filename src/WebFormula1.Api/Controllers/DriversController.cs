using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebFormula1.Core.Dtos;
using WebFormula1.Infrastructure.Data;

namespace WebFormula1.Api.Controllers;

[ApiController]
[Route("api/drivers")]
public class DriversController : ControllerBase
{
    private readonly Formula1DbContext _db;

    public DriversController(Formula1DbContext db) => _db = db;

    [HttpGet("{year:int}/stats")]
    public async Task<List<DriverStatsDto>> Stats(int year, CancellationToken ct)
    {
        var rows = await _db.DriverResults
            .Where(r => r.Race.SeasonYear == year)
            .Select(r => new
            {
                r.DriverId,
                Name = r.Driver.GivenName + " " + r.Driver.FamilyName,
                r.Driver.Code,
                Team = r.Team != null ? r.Team.Name : null,
                r.Position, r.Grid, r.Points, r.Status, r.FastestLapRank,
                r.Race.Round
            })
            .ToListAsync(ct);

        return rows
            .GroupBy(r => r.DriverId)
            .Select(g =>
            {
                var latest = g.OrderByDescending(x => x.Round).First();
                return new DriverStatsDto(
                    g.Key, latest.Name, latest.Code, latest.Team,
                    Races: g.Count(),
                    Wins: g.Count(x => x.Position == 1),
                    Podiums: g.Count(x => x.Position <= 3),
                    Poles: g.Count(x => x.Grid == 1),
                    FastestLaps: g.Count(x => x.FastestLapRank == 1),
                    Dnfs: g.Count(x => !(x.Status == "Finished" || x.Status.StartsWith("+") || x.Status == "Lapped")),
                    Points: g.Sum(x => x.Points),
                    BestFinish: g.Min(x => x.Position));
            })
            .OrderByDescending(s => s.Points).ThenByDescending(s => s.Wins)
            .ToList();
    }

    [HttpGet("{year:int}")]
    public async Task<List<DriverDto>> BySeason(int year, CancellationToken ct) =>
        await _db.Drivers
            .Where(d => d.Standings.Any(s => s.SeasonYear == year))
            .OrderBy(d => d.FamilyName)
            .Select(d => new DriverDto(
                d.Id, d.GivenName + " " + d.FamilyName, d.Code, d.PermanentNumber, d.Nationality, d.DateOfBirth,
                d.Team != null ? d.Team.Name : null))
            .ToListAsync(ct);
}
