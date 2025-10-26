namespace ENOC.Application.DTOs.CrewVehicleListing;

public class CrewVehicleListingResponse
{
    public Guid Id { get; set; }
    public string CrewVehicleListingFormId { get; set; } = string.Empty;
    public int CrewVehicleListingFormCounter { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmployeeId { get; set; } = string.Empty;
    public Guid TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<TeamStatusResponse> TeamStatuses { get; set; } = new();
    public List<VehicleStatusResponse> VehicleStatuses { get; set; } = new();
    public List<SCBAStatusResponse> SCBAStatuses { get; set; } = new();
    public List<RadioStatusResponse> RadioStatuses { get; set; } = new();
}

public class TeamStatusResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public Guid EmployeeStatusId { get; set; }
    public string EmployeeStatusName { get; set; } = string.Empty;
    public Guid? VehicleId { get; set; }
    public string? VehicleName { get; set; }
    public string? Remarks { get; set; }
}

public class VehicleStatusResponse
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public string VehicleName { get; set; } = string.Empty;
    public bool In { get; set; }
    public bool Out { get; set; }
    public bool Rs { get; set; }
    public string? Remarks { get; set; }
}

public class SCBAStatusResponse
{
    public Guid Id { get; set; }
    public Guid SCBAId { get; set; }
    public string SCBAName { get; set; } = string.Empty;
    public Guid? VehicleId { get; set; }
    public string? VehicleName { get; set; }
    public string? CylinderPressure { get; set; }
    public bool In { get; set; }
    public bool Out { get; set; }
    public bool Rs { get; set; }
    public string? Remarks { get; set; }
}

public class RadioStatusResponse
{
    public Guid Id { get; set; }
    public Guid RadioId { get; set; }
    public string RadioName { get; set; } = string.Empty;
    public bool In { get; set; }
    public bool Out { get; set; }
    public bool Rs { get; set; }
    public string? Remarks { get; set; }
}
