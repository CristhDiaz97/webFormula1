using Microsoft.AspNetCore.Mvc;
using WebFormula1.Infrastructure.Services;

namespace WebFormula1.Api.Controllers;

[ApiController]
[Route("api/sync")]
public class SyncController : ControllerBase
{
    private readonly IFormula1SyncService _sync;
    private readonly INotificationService _notifications;

    public SyncController(IFormula1SyncService sync, INotificationService notifications)
    {
        _sync = sync;
        _notifications = notifications;
    }

    [HttpPost("season/{year:int}")]
    public async Task<IActionResult> Season(int year, CancellationToken ct)
    {
        await _sync.SyncSeasonAsync(year, ct);
        var notifications = await _notifications.ScheduleAsync(ct);
        return Ok(new { message = $"Temporada {year} sincronizada", notifications });
    }

    [HttpPost("schedule/{year:int}")]
    public async Task<IActionResult> Schedule(int year, CancellationToken ct)
    {
        await _sync.SyncScheduleAsync(year, ct);
        return Ok(new { message = $"Calendario {year} sincronizado" });
    }

    [HttpPost("teams-drivers/{year:int}")]
    public async Task<IActionResult> TeamsAndDrivers(int year, CancellationToken ct)
    {
        await _sync.SyncTeamsAndDriversAsync(year, ct);
        return Ok(new { message = $"Equipos y pilotos {year} sincronizados" });
    }

    [HttpPost("standings/{year:int}")]
    public async Task<IActionResult> Standings(int year, CancellationToken ct)
    {
        await _sync.SyncStandingsAsync(year, ct);
        return Ok(new { message = $"Clasificaciones {year} sincronizadas" });
    }

    [HttpPost("results/{year:int}")]
    public async Task<IActionResult> Results(int year, CancellationToken ct)
    {
        await _sync.SyncResultsAsync(year, ct);
        return Ok(new { message = $"Resultados {year} sincronizados" });
    }
}
