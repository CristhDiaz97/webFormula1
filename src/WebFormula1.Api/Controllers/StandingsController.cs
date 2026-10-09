using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebFormula1.Core.Dtos;
using WebFormula1.Infrastructure.Data;

namespace WebFormula1.Api.Controllers;

[ApiController]
[Route("api/standings")]
public class StandingsController : ControllerBase
{
    private readonly Formula1DbContext _db;

    public StandingsController(Formula1DbContext db) => _db = db;

    [HttpGet("drivers/{year:int}")]
    public async Task<List<DriverStandingDto>> Drivers(int year, CancellationToken ct) =>
        await _db.DriverStandings
            .Where(s => s.SeasonYear == year)
            .OrderBy(s => s.Position)
            .Select(s => new DriverStandingDto(
                s.Position, s.Points, s.Wins, s.Driver.GivenName + " " + s.Driver.FamilyName, s.Driver.Code,
                s.Driver.PermanentNumber, s.Driver.Team != null ? s.Driver.Team.Name : "", s.Driver.Nationality))
            .ToListAsync(ct);

    [HttpGet("constructors/{year:int}")]
    public async Task<List<ConstructorStandingDto>> Constructors(int year, CancellationToken ct) =>
        await _db.TeamStandings
            .Where(s => s.SeasonYear == year)
            .OrderBy(s => s.Position)
            .Select(s => new ConstructorStandingDto(s.Position, s.Points, s.Wins, s.TeamId, s.Team.Name, s.Team.Nationality))
            .ToListAsync(ct);
}
