using WebFormula1.Core.Models;

namespace WebFormula1.Core.Dtos;

public static class ColombiaTime
{
    // Colombia es UTC-5 todo el año (no tiene horario de verano).
    public static readonly TimeSpan Offset = TimeSpan.FromHours(-5);

    private static readonly string[] Days = { "dom", "lun", "mar", "mié", "jue", "vie", "sáb" };
    private static readonly string[] Months =
        { "ene", "feb", "mar", "abr", "may", "jun", "jul", "ago", "sep", "oct", "nov", "dic" };

    public static DateTimeOffset FromUtc(DateTime utc) =>
        new DateTimeOffset(DateTime.SpecifyKind(utc, DateTimeKind.Utc)).ToOffset(Offset);

    public static string Format(DateTimeOffset value) =>
        $"{Days[(int)value.DayOfWeek]} {value.Day} {Months[value.Month - 1]}, {value:HH:mm}";

    public static RaceDto ToDto(Race race, string? winner) => new(
        race.Id, race.SeasonYear, race.Round, race.Name, race.Circuit, race.Locality, race.Country,
        race.RaceStartUtc.AddHours(3) < DateTime.UtcNow, winner,
        race.GetSessions().Select(s => new SessionDto(s.Name, FromUtc(s.Utc))).ToList());
}
