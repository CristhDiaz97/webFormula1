namespace WebFormula1.Core.Models;

public class Team
{
    public int Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string? History { get; set; }

    public ICollection<Driver> Drivers { get; set; } = new List<Driver>();
    public ICollection<TeamStanding> Standings { get; set; } = new List<TeamStanding>();
}

public class Driver
{
    public int Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string GivenName { get; set; } = string.Empty;
    public string FamilyName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string Nationality { get; set; } = string.Empty;
    public string? PermanentNumber { get; set; }
    public string? Url { get; set; }

    public int? TeamId { get; set; }
    public Team? Team { get; set; }
    public ICollection<DriverResult> Results { get; set; } = new List<DriverResult>();
    public ICollection<DriverStanding> Standings { get; set; } = new List<DriverStanding>();

    public string FullName => $"{GivenName} {FamilyName}";
}

public class Race
{
    public int Id { get; set; }
    public int SeasonYear { get; set; }
    public int Round { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Circuit { get; set; } = string.Empty;
    public string Locality { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? Url { get; set; }

    public DateTime RaceStartUtc { get; set; }
    public DateTime? FirstPracticeUtc { get; set; }
    public DateTime? SecondPracticeUtc { get; set; }
    public DateTime? ThirdPracticeUtc { get; set; }
    public DateTime? QualifyingUtc { get; set; }
    public DateTime? SprintQualifyingUtc { get; set; }
    public DateTime? SprintUtc { get; set; }

    public ICollection<DriverResult> Results { get; set; } = new List<DriverResult>();

    public IReadOnlyList<(string Name, DateTime Utc)> GetSessions()
    {
        var list = new List<(string, DateTime)>();
        if (FirstPracticeUtc is { } fp1) list.Add(("Libres 1", fp1));
        if (SprintQualifyingUtc is { } sq) list.Add(("Clasificación Sprint", sq));
        if (SecondPracticeUtc is { } fp2) list.Add(("Libres 2", fp2));
        if (ThirdPracticeUtc is { } fp3) list.Add(("Libres 3", fp3));
        if (SprintUtc is { } sp) list.Add(("Sprint", sp));
        if (QualifyingUtc is { } q) list.Add(("Clasificación", q));
        list.Add(("Carrera", RaceStartUtc));
        return list.OrderBy(s => s.Item2).ToList();
    }

    public DateTime FirstSessionUtc => GetSessions()[0].Utc;
}

public class DriverResult
{
    public int Id { get; set; }
    public int RaceId { get; set; }
    public Race Race { get; set; } = null!;
    public int DriverId { get; set; }
    public Driver Driver { get; set; } = null!;
    public int? TeamId { get; set; }
    public Team? Team { get; set; }

    public int Position { get; set; }
    public string PositionText { get; set; } = string.Empty;
    public int Grid { get; set; }
    public int Laps { get; set; }
    public double Points { get; set; }
    public string? TimeText { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? FastestLapRank { get; set; }
}

public class DriverStanding
{
    public int Id { get; set; }
    public int SeasonYear { get; set; }
    public int Round { get; set; }
    public int DriverId { get; set; }
    public Driver Driver { get; set; } = null!;
    public int Position { get; set; }
    public double Points { get; set; }
    public int Wins { get; set; }
}

public class TeamStanding
{
    public int Id { get; set; }
    public int SeasonYear { get; set; }
    public int Round { get; set; }
    public int TeamId { get; set; }
    public Team Team { get; set; } = null!;
    public int Position { get; set; }
    public double Points { get; set; }
    public int Wins { get; set; }
}

public class RaceNotification
{
    public int Id { get; set; }
    public int RaceId { get; set; }
    public Race Race { get; set; } = null!;
    public DateTime NotifyAtUtc { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool Sent { get; set; }
    public DateTime? SentAtUtc { get; set; }
}
