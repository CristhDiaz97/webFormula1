using Microsoft.EntityFrameworkCore;
using WebFormula1.Infrastructure.Api;
using WebFormula1.Infrastructure.Data;
using WebFormula1.Infrastructure.Services;

namespace WebFormula1.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<Formula1DbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddHttpClient<IJolpicaClient, JolpicaClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["JolpicaF1:BaseUrl"] ?? "https://api.jolpi.ca/");
            client.Timeout = TimeSpan.FromSeconds(60);
        });

        services.AddHttpClient<IWikipediaClient, WikipediaClient>(client =>
        {
            client.BaseAddress = new Uri("https://en.wikipedia.org/");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("WebFormula1/1.0 (proyecto local de desarrollo)");
            client.Timeout = TimeSpan.FromSeconds(20);
        });

        services.AddScoped<IFormula1SyncService, Formula1SyncService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<INotificationProvider, ConsoleNotificationProvider>();
        services.AddScoped<IScheduledJobs, ScheduledJobs>();

        return services;
    }

    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddCors(options => options.AddPolicy("AllowAll", policy =>
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

        return services;
    }
}
