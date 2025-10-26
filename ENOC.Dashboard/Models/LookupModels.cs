namespace ENOC.Dashboard.Models;

public class BusinessUnit
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class IncidentType
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Image { get; set; }
}

public class Message
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class Tank
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TankNumber { get; set; }
    public string BusinessUnit { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? ERG { get; set; }
}

public class Team
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class Vehicle
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? PlateNumber { get; set; }
}

public class EercPosition
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
