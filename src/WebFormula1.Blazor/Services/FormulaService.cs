using System.Net;
using System.Net.Http.Json;
using WebFormula1.Core.Dtos;

namespace WebFormula1.Blazor.Services;

public class FormulaService
{
    private readonly HttpClient _http;

    public FormulaService(HttpClient http) => _http = http;

    public async Task<List<int>> GetSeasonsAsync() =>
        await _http.GetFromJsonAsync<List<int>>("api/races/seasons") ?? new();

    public async Task<RaceDto?> GetNextRaceAsync()
    {
        using var response = await _http.GetAsync("api/races/next");
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RaceDto>();
    }

    public async Task<List<NotificationDto>> GetActiveNotificationsAsync() =>
        await _http.GetFromJsonAsync<List<NotificationDto>>("api/notifications/active") ?? new();

    public async Task<List<RaceDto>> GetRacesAsync(int year) =>
        await _http.GetFromJsonAsync<List<RaceDto>>($"api/races/{year}") ?? new();

    public Task<RaceDetailDto?> GetRaceDetailAsync(int year, int round) =>
        _http.GetFromJsonAsync<RaceDetailDto>($"api/races/{year}/{round}");

    public async Task<List<DriverStandingDto>> GetDriverStandingsAsync(int year) =>
        await _http.GetFromJsonAsync<List<DriverStandingDto>>($"api/standings/drivers/{year}") ?? new();

    public async Task<List<ConstructorStandingDto>> GetConstructorStandingsAsync(int year) =>
        await _http.GetFromJsonAsync<List<ConstructorStandingDto>>($"api/standings/constructors/{year}") ?? new();

    public async Task<List<DriverStatsDto>> GetDriverStatsAsync(int year) =>
        await _http.GetFromJsonAsync<List<DriverStatsDto>>($"api/drivers/{year}/stats") ?? new();

    public async Task<List<TeamDto>> GetTeamsAsync(int year) =>
        await _http.GetFromJsonAsync<List<TeamDto>>($"api/teams/{year}") ?? new();
}
