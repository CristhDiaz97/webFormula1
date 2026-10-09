using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebFormula1.Infrastructure.Api;
using WebFormula1.Infrastructure.Data;

namespace WebFormula1.Api.Controllers;

[ApiController]
[Route("api/apistatus")]
public class ApiStatusController : ControllerBase
{
    private readonly IJolpicaClient _jolpica;
    private readonly Formula1DbContext _db;

    public ApiStatusController(IJolpicaClient jolpica, Formula1DbContext db)
    {
        _jolpica = jolpica;
        _db = db;
    }

    [HttpGet("health")]
    public async Task<IActionResult> Health(CancellationToken ct)
    {
        try
        {
            var ok = await _jolpica.PingAsync(ct);
            return ok
                ? Ok(new { isHealthy = true, message = "Jolpica F1 responde correctamente" })
                : StatusCode(503, new { isHealthy = false, message = "Jolpica F1 respondió con error" });
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return StatusCode(503, new { isHealthy = false, message = $"No se pudo conectar con Jolpica F1: {ex.Message}" });
        }
    }

    [HttpGet("status")]
    public async Task<IActionResult> Status(CancellationToken ct) => Ok(new
    {
        timestamp = DateTime.UtcNow,
        races = await _db.Races.CountAsync(ct),
        drivers = await _db.Drivers.CountAsync(ct),
        teams = await _db.Teams.CountAsync(ct),
        results = await _db.DriverResults.CountAsync(ct)
    });
}
