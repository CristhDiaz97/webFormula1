using Microsoft.AspNetCore.Mvc;
using WebFormula1.Core.Dtos;
using WebFormula1.Infrastructure.Services;

namespace WebFormula1.Api.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notifications;

    public NotificationsController(INotificationService notifications) => _notifications = notifications;

    [HttpGet("active")]
    public Task<List<NotificationDto>> Active(CancellationToken ct) => _notifications.GetActiveAsync(ct);

    [HttpPost("schedule")]
    public async Task<IActionResult> Schedule(CancellationToken ct) =>
        Ok(new { changed = await _notifications.ScheduleAsync(ct) });
}
