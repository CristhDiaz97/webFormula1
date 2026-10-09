using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebFormula1.Core.Dtos;
using WebFormula1.Core.Models;
using WebFormula1.Infrastructure.Data;

namespace WebFormula1.Infrastructure.Services;

public interface INotificationProvider
{
    Task SendAsync(string title, string message, CancellationToken ct);
}

public class ConsoleNotificationProvider : INotificationProvider
{
    private readonly ILogger<ConsoleNotificationProvider> _logger;

    public ConsoleNotificationProvider(ILogger<ConsoleNotificationProvider> logger) => _logger = logger;

    public Task SendAsync(string title, string message, CancellationToken ct)
    {
        _logger.LogInformation("NOTIFICACIÓN {Title}\n{Message}", title, message);
        return Task.CompletedTask;
    }
}

public interface INotificationService
{
    Task<int> ScheduleAsync(CancellationToken ct);
    Task<int> SendPendingAsync(CancellationToken ct);
    Task<List<NotificationDto>> GetActiveAsync(CancellationToken ct);
}

public class NotificationService : INotificationService
{
    private static readonly TimeSpan RaceDuration = TimeSpan.FromHours(3);
    private static readonly TimeSpan LeadTime = TimeSpan.FromHours(24);

    private readonly Formula1DbContext _db;
    private readonly INotificationProvider _provider;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(Formula1DbContext db, INotificationProvider provider, ILogger<NotificationService> logger)
    {
        _db = db;
        _provider = provider;
        _logger = logger;
    }

    // Crea (o actualiza) un aviso por fin de semana, 24 h antes de la primera sesión.
    public async Task<int> ScheduleAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var upcoming = await _db.Races.Where(r => r.RaceStartUtc > now - RaceDuration).ToListAsync(ct);
        var existing = await _db.RaceNotifications.ToDictionaryAsync(n => n.RaceId, ct);
        var changed = 0;

        foreach (var race in upcoming)
        {
            if (race.FirstSessionUtc <= now) continue;

            var notifyAt = race.FirstSessionUtc - LeadTime;
            var message = BuildMessage(race);

            if (!existing.TryGetValue(race.Id, out var notification))
            {
                _db.RaceNotifications.Add(new RaceNotification { Race = race, NotifyAtUtc = notifyAt, Message = message });
                changed++;
            }
            else if (!notification.Sent && (notification.NotifyAtUtc != notifyAt || notification.Message != message))
            {
                notification.NotifyAtUtc = notifyAt;
                notification.Message = message;
                changed++;
            }
        }

        await _db.SaveChangesAsync(ct);
        return changed;
    }

    public async Task<int> SendPendingAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var pending = await _db.RaceNotifications
            .Include(n => n.Race)
            .Where(n => !n.Sent && n.NotifyAtUtc <= now && n.Race.RaceStartUtc > now - RaceDuration)
            .ToListAsync(ct);

        foreach (var n in pending)
        {
            await _provider.SendAsync($"Fórmula 1 - {n.Race.Name}", n.Message, ct);
            n.Sent = true;
            n.SentAtUtc = now;
        }

        await _db.SaveChangesAsync(ct);
        return pending.Count;
    }

    public async Task<List<NotificationDto>> GetActiveAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var active = await _db.RaceNotifications
            .Include(n => n.Race)
            .Where(n => n.NotifyAtUtc <= now && n.Race.RaceStartUtc > now - RaceDuration)
            .OrderBy(n => n.NotifyAtUtc)
            .ToListAsync(ct);

        return active
            .Select(n => new NotificationDto(n.Id, n.Race.Name, ColombiaTime.FromUtc(n.NotifyAtUtc), n.Message, n.Sent))
            .ToList();
    }

    private static string BuildMessage(Race race)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{race.Name} - este fin de semana (hora de Colombia)");
        foreach (var (name, utc) in race.GetSessions())
            sb.AppendLine($"{name}: {ColombiaTime.Format(ColombiaTime.FromUtc(utc))}");
        return sb.ToString().TrimEnd();
    }
}
