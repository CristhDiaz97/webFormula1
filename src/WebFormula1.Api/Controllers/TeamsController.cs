using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebFormula1.Core.Dtos;
using WebFormula1.Infrastructure.Data;

namespace WebFormula1.Api.Controllers;

[ApiController]
[Route("api/teams")]
public class TeamsController : ControllerBase
{
    private readonly Formula1DbContext _db;

    public TeamsController(Formula1DbContext db) => _db = db;

    [HttpGet("{year:int}")]
    public async Task<List<TeamDto>> BySeason(int year, CancellationToken ct)
    {
        var teams = await _db.Teams
            .Where(t => t.Standings.Any(s => s.SeasonYear == year))
            .OrderBy(t => t.Name)
            .Select(t => new
            {
                t.Id, t.Name, t.Nationality, t.History, t.Url,
                Drivers = t.Drivers
                    .Where(d => d.Standings.Any(s => s.SeasonYear == year))
                    .Select(d => d.GivenName + " " + d.FamilyName)
                    .ToList()
            })
            .ToListAsync(ct);

        return teams.Select(t => new TeamDto(t.Id, t.Name, t.Nationality, t.History, t.Url, t.Drivers)).ToList();
    }
}
