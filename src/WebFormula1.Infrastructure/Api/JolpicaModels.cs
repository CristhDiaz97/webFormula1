namespace WebFormula1.Infrastructure.Api;

// Formato Ergast que devuelve https://api.jolpi.ca/ergast/f1/ (todos los números llegan como texto).

public class ErgastResponse
{
    public MRData MRData { get; set; } = new();
}

public class MRData
{
    public string Total { get; set; } = "0";
    public RaceTable? RaceTable { get; set; }
    public StandingsTable? StandingsTable { get; set; }
    public DriverTable? DriverTable { get; set; }
    public ConstructorTable? ConstructorTable { get; set; }
}

public class RaceTable { public List<ErgastRace> Races { get; set; } = new(); }
public class DriverTable { public List<ErgastDriver> Drivers { get; set; } = new(); }
public class ConstructorTable { public List<ErgastConstructor> Constructors { get; set; } = new(); }
public class StandingsTable { public List<StandingsList> StandingsLists { get; set; } = new(); }

public class ErgastRace
{
    public string Season { get; set; } = "";
    public string Round { get; set; } = "";
    public string RaceName { get; set; } = "";
    public string? Url { get; set; }
    public ErgastCircuit Circuit { get; set; } = new();
    public string Date { get; set; } = "";
    public string? Time { get; set; }
    public ErgastSession? FirstPractice { get; set; }
    public ErgastSession? SecondPractice { get; set; }
    public ErgastSession? ThirdPractice { get; set; }
    public ErgastSession? Qualifying { get; set; }
    public ErgastSession? Sprint { get; set; }
    public ErgastSession? SprintQualifying { get; set; }
    public ErgastSession? SprintShootout { get; set; }
    public List<ErgastResult>? Results { get; set; }
}

public class ErgastSession
{
    public string Date { get; set; } = "";
    public string? Time { get; set; }
}

public class ErgastCircuit
{
    public string CircuitId { get; set; } = "";
    public string CircuitName { get; set; } = "";
    public ErgastLocation Location { get; set; } = new();
}

public class ErgastLocation
{
    public string Locality { get; set; } = "";
    public string Country { get; set; } = "";
}

public class ErgastDriver
{
    public string DriverId { get; set; } = "";
    public string? PermanentNumber { get; set; }
    public string? Code { get; set; }
    public string? Url { get; set; }
    public string GivenName { get; set; } = "";
    public string FamilyName { get; set; } = "";
    public string? DateOfBirth { get; set; }
    public string Nationality { get; set; } = "";
}

public class ErgastConstructor
{
    public string ConstructorId { get; set; } = "";
    public string? Url { get; set; }
    public string Name { get; set; } = "";
    public string Nationality { get; set; } = "";
}

public class StandingsList
{
    public string Round { get; set; } = "0";
    public List<ErgastDriverStanding>? DriverStandings { get; set; }
    public List<ErgastConstructorStanding>? ConstructorStandings { get; set; }
}

public class ErgastDriverStanding
{
    public string Position { get; set; } = "0";
    public string Points { get; set; } = "0";
    public string Wins { get; set; } = "0";
    public ErgastDriver Driver { get; set; } = new();
    public List<ErgastConstructor> Constructors { get; set; } = new();
}

public class ErgastConstructorStanding
{
    public string Position { get; set; } = "0";
    public string Points { get; set; } = "0";
    public string Wins { get; set; } = "0";
    public ErgastConstructor Constructor { get; set; } = new();
}

public class ErgastResult
{
    public string Position { get; set; } = "0";
    public string PositionText { get; set; } = "";
    public string Points { get; set; } = "0";
    public ErgastDriver Driver { get; set; } = new();
    public ErgastConstructor Constructor { get; set; } = new();
    public string Grid { get; set; } = "0";
    public string Laps { get; set; } = "0";
    public string Status { get; set; } = "";
    public ErgastResultTime? Time { get; set; }
    public ErgastFastestLap? FastestLap { get; set; }
}

public class ErgastResultTime { public string? Time { get; set; } }
public class ErgastFastestLap { public string? Rank { get; set; } }
