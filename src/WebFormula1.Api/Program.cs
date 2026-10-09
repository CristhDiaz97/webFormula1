using Hangfire;
using Microsoft.EntityFrameworkCore;
using Serilog;
using WebFormula1.Api.Extensions;
using WebFormula1.Api.Middleware;
using WebFormula1.Infrastructure.Data;
using WebFormula1.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("System.Net.Http.HttpClient", Serilog.Events.LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File("logs/formula1-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApiServices();
builder.Services.AddHangfire(config => config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandler>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();
app.UseHangfireDashboard();

bool needsInitialSync;
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<Formula1DbContext>();
    db.Database.Migrate();
    needsInitialSync = !db.Races.Any(r => r.SeasonYear == DateTime.UtcNow.Year);
}

RecurringJob.AddOrUpdate<IScheduledJobs>("sync-current-season", j => j.SyncCurrentSeasonAsync(), "0 */3 * * *");
RecurringJob.AddOrUpdate<IScheduledJobs>("send-notifications", j => j.SendNotificationsAsync(), "*/15 * * * *");

if (needsInitialSync)
    BackgroundJob.Enqueue<IScheduledJobs>(j => j.SyncCurrentSeasonAsync());

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación terminó inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}
