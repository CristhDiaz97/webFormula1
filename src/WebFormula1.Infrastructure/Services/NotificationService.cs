using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebFormula1.Core.Dtos;
using WebFormula1.Core.Models;
using WebFormula1.Infrastructure.Data;

namespace WebFormula1.Infrastructure.Services;

public record ProviderTestResult(string Provider, bool Configured, bool Ok, string? Error);

public interface INotificationService
{
    Task<int> ScheduleAsync(CancellationToken ct);
    Task<int> SendPendingAsync(CancellationToken ct);
    Task<List<NotificationDto>> GetActiveAsync(CancellationToken ct);
    Task<List<ProviderTestResult>> SendTestAsync(CancellationToken ct);
}

public class NotificationService : INotificationService
{
    private static readonly TimeSpan RaceDuration = TimeSpan.FromHours(3);
    private static readonly TimeSpan WeekendLeadTime = TimeSpan.FromHours(24);
    private static readonly TimeSpan ReminderMaxDelay = TimeSpan.FromMinutes(10);

    private readonly Formula1DbContext _db;
    private readonly IEnumerable<INotificationProvider> _providers;
    private readonly NotificationOptions _options;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        Formula1DbContext db,
        IEnumerable<INotificationProvider> providers,
        IOptions<NotificationOptions> options,
        ILogger<NotificationService> logger)
    {
        _db = db;
        _providers = providers;
        _options = options.Value;
        _logger = logger;
    }

    // Crea o actualiza el aviso del fin de semana (24 h antes) y un recordatorio por cada sesión futura.
    public async Task<int> ScheduleAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var lead = TimeSpan.FromMinutes(Math.Max(1, _options.SessionLeadMinutes));
        var upcoming = await _db.Races.Where(r => r.RaceStartUtc > now - RaceDuration).ToListAsync(ct);
        var weekend = await _db.RaceNotifications.ToDictionaryAsync(n => n.RaceId, ct);
        var reminders = await _db.SessionReminders.ToDictionaryAsync(r => (r.RaceId, r.SessionName), ct);
        var changed = 0;

        foreach (var race in upcoming)
        {
            if (race.FirstSessionUtc > now)
            {
                var notifyAt = race.FirstSessionUtc - WeekendLeadTime;
                var message = BuildWeekendMessage(race);

                if (!weekend.TryGetValue(race.Id, out var notification))
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

            foreach (var (name, startUtc) in race.GetSessions())
            {
                if (startUtc <= now) continue;

                var notifyAt = startUtc - lead;
                if (!reminders.TryGetValue((race.Id, name), out var reminder))
                {
                    _db.SessionReminders.Add(new SessionReminder
                    {
                        Race = race, SessionName = name, StartUtc = startUtc, NotifyAtUtc = notifyAt
                    });
                    changed++;
                }
                else if (!reminder.Sent && (reminder.StartUtc != startUtc || reminder.NotifyAtUtc != notifyAt))
                {
                    reminder.StartUtc = startUtc;
                    reminder.NotifyAtUtc = notifyAt;
                    changed++;
                }
            }
        }

        await _db.SaveChangesAsync(ct);
        return changed;
    }

    public async Task<int> SendPendingAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var sent = 0;

        var weekend = await _db.RaceNotifications
            .Include(n => n.Race)
            .Where(n => !n.Sent && n.NotifyAtUtc <= now && n.Race.RaceStartUtc > now - RaceDuration)
            .ToListAsync(ct);

        foreach (var n in weekend)
        {
            if (!await TrySendAsync($"Fórmula 1 - {n.Race.Name}", n.Message, ct)) continue;
            n.Sent = true;
            n.SentAtUtc = now;
            sent++;
        }

        var due = await _db.SessionReminders
            .Include(r => r.Race)
            .Where(r => !r.Sent && r.NotifyAtUtc <= now)
            .ToListAsync(ct);

        foreach (var r in due)
        {
            // Si la API estuvo apagada y la sesión ya pasó hace rato, no tiene sentido avisar.
            if (r.StartUtc < now - ReminderMaxDelay)
            {
                r.Sent = true;
                r.SentAtUtc = now;
                _logger.LogInformation("Recordatorio omitido por tardío: {Session} de {Race}", r.SessionName, r.Race.Name);
                continue;
            }

            if (!await TrySendAsync($"Fórmula 1 - {r.SessionName}", BuildReminderMessage(r, now), ct)) continue;
            r.Sent = true;
            r.SentAtUtc = now;
            sent++;
        }

        await _db.SaveChangesAsync(ct);
        return sent;
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

    public async Task<List<ProviderTestResult>> SendTestAsync(CancellationToken ct)
    {
        var results = new List<ProviderTestResult>();
        foreach (var provider in _providers)
        {
            if (!provider.IsConfigured)
            {
                results.Add(new ProviderTestResult(provider.Name, false, false, "No configurado"));
                continue;
            }

            try
            {
                await provider.SendAsync("Fórmula 1 - prueba", "Si lees esto, las notificaciones funcionan.", ct);
                results.Add(new ProviderTestResult(provider.Name, true, true, null));
            }
            catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
            {
                results.Add(new ProviderTestResult(provider.Name, true, false, ex.Message));
            }
        }

        return results;
    }

    // Devuelve true solo si todos los canales configurados lo enviaron; si no, se reintenta en la siguiente pasada.
    private async Task<bool> TrySendAsync(string title, string message, CancellationToken ct)
    {
        var ok = true;
        foreach (var provider in _providers.Where(p => p.IsConfigured))
        {
            try
            {
                await provider.SendAsync(title, message, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
            {
                ok = false;
                _logger.LogWarning("No se pudo enviar por {Provider}: {Error}", provider.Name, ex.Message);
            }
        }

        return ok;
    }

    private static string BuildWeekendMessage(Race race)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{race.Name} - este fin de semana (hora de Colombia)");
        foreach (var (name, utc) in race.GetSessions())
            sb.AppendLine($"{name}: {ColombiaTime.Format(ColombiaTime.FromUtc(utc))}");
        return sb.ToString().TrimEnd();
    }

    private static string BuildReminderMessage(SessionReminder reminder, DateTime now)
    {
        var left = reminder.StartUtc - now;
        var when = left <= TimeSpan.Zero
            ? "está empezando ahora"
            : $"empieza en {Math.Max(1, (int)Math.Ceiling(left.TotalMinutes))} min";

        return $"{reminder.SessionName} del {reminder.Race.Name} {when}.\n" +
               $"Hora de Colombia: {ColombiaTime.Format(ColombiaTime.FromUtc(reminder.StartUtc))}";
    }
}
