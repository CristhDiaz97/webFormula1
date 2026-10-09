namespace WebFormula1.Core.Dtos;

public record SessionDto(string Name, DateTimeOffset StartColombia);

public record RaceDto(
    int Id, int Season, int Round, string Name, string Circuit, string Locality, string Country,
    bool IsCompleted, string? Winner, List<SessionDto> Sessions);

public record RaceResultDto(
    int Position, string PositionText, string Driver, string Code, string Team,
    int Grid, int Laps, string? Time, string Status, double Points, bool FastestLap);

public record RaceDetailDto(RaceDto Race, List<RaceResultDto> Results);

public record DriverStandingDto(
    int Position, double Points, int Wins, string Driver, string Code, string? Number, string Team, string Nationality);

public record ConstructorStandingDto(
    int Position, double Points, int Wins, int TeamId, string Team, string Nationality);

public record DriverDto(
    int Id, string Name, string Code, string? Number, string Nationality, DateTime? DateOfBirth, string? Team);

public record DriverStatsDto(
    int DriverId, string Name, string Code, string? Team, int Races, int Wins, int Podiums,
    int Poles, int FastestLaps, int Dnfs, double Points, int BestFinish);

public record TeamDto(int Id, string Name, string Nationality, string? History, string? Url, List<string> Drivers);

public record NotificationDto(int Id, string Race, DateTimeOffset NotifyAtColombia, string Message, bool Sent);
