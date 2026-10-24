using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WebFormula1.Infrastructure.Services;

public class TelegramOptions
{
    public string? BotToken { get; set; }
    public string? ChatId { get; set; }
}

public class NotificationOptions
{
    public int SessionLeadMinutes { get; set; } = 15;
}

public interface INotificationProvider
{
    string Name { get; }
    bool IsConfigured { get; }
    Task SendAsync(string title, string message, CancellationToken ct);
}

public class ConsoleNotificationProvider : INotificationProvider
{
    private readonly ILogger<ConsoleNotificationProvider> _logger;

    public ConsoleNotificationProvider(ILogger<ConsoleNotificationProvider> logger) => _logger = logger;

    public string Name => "Log";
    public bool IsConfigured => true;

    public Task SendAsync(string title, string message, CancellationToken ct)
    {
        _logger.LogInformation("NOTIFICACIÓN {Title}\n{Message}", title, message);
        return Task.CompletedTask;
    }
}

public class TelegramNotificationProvider : INotificationProvider
{
    private readonly HttpClient _http;
    private readonly TelegramOptions _options;

    public TelegramNotificationProvider(HttpClient http, IOptions<TelegramOptions> options)
    {
        _http = http;
        _options = options.Value;
    }

    public string Name => "Telegram";

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_options.BotToken) && !string.IsNullOrWhiteSpace(_options.ChatId);

    public async Task SendAsync(string title, string message, CancellationToken ct)
    {
        // El token va en la URL, por eso el log de HttpClient está en Warning y nunca se incluye la URL en los errores.
        using var response = await _http.PostAsJsonAsync(
            $"/bot{_options.BotToken}/sendMessage",
            new { chat_id = _options.ChatId, text = $"{title}\n\n{message}" },
            ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"Telegram respondió {(int)response.StatusCode}: {body}");
        }
    }
}
