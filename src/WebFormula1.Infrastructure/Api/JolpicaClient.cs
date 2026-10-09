using System.Globalization;
using System.Net;
using System.Net.Http.Json;

namespace WebFormula1.Infrastructure.Api;

public interface IJolpicaClient
{
    Task<bool> PingAsync(CancellationToken ct);
    Task<List<ErgastRace>> GetScheduleAsync(int year, CancellationToken ct);
    Task<List<ErgastDriver>> GetDriversAsync(int year, CancellationToken ct);
    Task<List<ErgastConstructor>> GetConstructorsAsync(int year, CancellationToken ct);
    Task<StandingsList?> GetDriverStandingsAsync(int year, CancellationToken ct);
    Task<StandingsList?> GetConstructorStandingsAsync(int year, CancellationToken ct);
    Task<List<ErgastRace>> GetResultsAsync(int year, CancellationToken ct);
}

public class JolpicaClient : IJolpicaClient
{
    private const int PageSize = 100;
    private readonly HttpClient _http;

    public JolpicaClient(HttpClient http) => _http = http;

    public async Task<bool> PingAsync(CancellationToken ct)
    {
        using var response = await _http.GetAsync("ergast/f1/current.json?limit=1", ct);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<ErgastRace>> GetScheduleAsync(int year, CancellationToken ct) =>
        (await GetAsync($"ergast/f1/{year}.json?limit={PageSize}", ct)).MRData.RaceTable?.Races ?? new();

    public async Task<List<ErgastDriver>> GetDriversAsync(int year, CancellationToken ct) =>
        (await GetAsync($"ergast/f1/{year}/drivers.json?limit={PageSize}", ct)).MRData.DriverTable?.Drivers ?? new();

    public async Task<List<ErgastConstructor>> GetConstructorsAsync(int year, CancellationToken ct) =>
        (await GetAsync($"ergast/f1/{year}/constructors.json?limit={PageSize}", ct)).MRData.ConstructorTable?.Constructors ?? new();

    public async Task<StandingsList?> GetDriverStandingsAsync(int year, CancellationToken ct) =>
        (await GetAsync($"ergast/f1/{year}/driverstandings.json?limit={PageSize}", ct))
            .MRData.StandingsTable?.StandingsLists.FirstOrDefault();

    public async Task<StandingsList?> GetConstructorStandingsAsync(int year, CancellationToken ct) =>
        (await GetAsync($"ergast/f1/{year}/constructorstandings.json?limit={PageSize}", ct))
            .MRData.StandingsTable?.StandingsLists.FirstOrDefault();

    // Los resultados vienen paginados y una carrera puede quedar partida entre dos páginas, así que se combinan por ronda.
    public async Task<List<ErgastRace>> GetResultsAsync(int year, CancellationToken ct)
    {
        var byRound = new SortedDictionary<int, ErgastRace>();
        var offset = 0;
        int total;
        do
        {
            var page = (await GetAsync($"ergast/f1/{year}/results.json?limit={PageSize}&offset={offset}", ct)).MRData;
            total = int.Parse(page.Total, CultureInfo.InvariantCulture);
            foreach (var race in page.RaceTable?.Races ?? new())
            {
                var round = int.Parse(race.Round, CultureInfo.InvariantCulture);
                if (byRound.TryGetValue(round, out var existing))
                    existing.Results!.AddRange(race.Results ?? new());
                else
                    byRound[round] = race;
            }
            offset += PageSize;
            await Task.Delay(300, ct);
        } while (offset < total);

        return byRound.Values.ToList();
    }

    private async Task<ErgastResponse> GetAsync(string path, CancellationToken ct)
    {
        for (var attempt = 1; attempt <= 4; attempt++)
        {
            using var response = await _http.GetAsync(path, ct);
            if (response.StatusCode == HttpStatusCode.TooManyRequests || (int)response.StatusCode >= 500)
            {
                await Task.Delay(TimeSpan.FromSeconds(2 * attempt), ct);
                continue;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ErgastResponse>(cancellationToken: ct)
                   ?? throw new InvalidOperationException($"Respuesta vacía de Jolpica para {path}");
        }

        throw new HttpRequestException($"Jolpica no respondió correctamente para {path} tras varios intentos.");
    }
}
