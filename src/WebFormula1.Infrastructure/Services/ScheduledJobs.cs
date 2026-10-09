namespace WebFormula1.Infrastructure.Services;

// Hangfire no admite argumentos opcionales en las expresiones, por eso los métodos no reciben parámetros.
public interface IScheduledJobs
{
    Task SyncCurrentSeasonAsync();
    Task SendNotificationsAsync();
}

public class ScheduledJobs : IScheduledJobs
{
    private readonly IFormula1SyncService _sync;
    private readonly INotificationService _notifications;

    public ScheduledJobs(IFormula1SyncService sync, INotificationService notifications)
    {
        _sync = sync;
        _notifications = notifications;
    }

    public async Task SyncCurrentSeasonAsync()
    {
        await _sync.SyncSeasonAsync(DateTime.UtcNow.Year, CancellationToken.None);
        await _notifications.ScheduleAsync(CancellationToken.None);
    }

    public async Task SendNotificationsAsync()
    {
        await _notifications.ScheduleAsync(CancellationToken.None);
        await _notifications.SendPendingAsync(CancellationToken.None);
    }
}
