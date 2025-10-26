using System.ComponentModel.DataAnnotations;

namespace ENOC.Application.DTOs.CrewVehicleListing;

public class CreateCrewVehicleListingRequest
{
    [Required]
    public Guid TeamId { get; set; }

    [Required]
    public DateTime From { get; set; }

    [Required]
    public DateTime To { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Details { get; set; }

    public List<TeamStatusEntryDto> TeamStatuses { get; set; } = new();
    public List<FireVehicleStatusEntryDto> VehicleStatuses { get; set; } = new();
    public List<SCBAStatusEntryDto> SCBAStatuses { get; set; } = new();
    public List<RadioStatusEntryDto> RadioStatuses { get; set; } = new();
}

public class TeamStatusEntryDto
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid EmployeeStatusId { get; set; }

    public Guid? VehicleId { get; set; }

    public string? Remarks { get; set; }
}

public class FireVehicleStatusEntryDto
{
    [Required]
    public Guid VehicleId { get; set; }

    public bool In { get; set; }

    public bool Out { get; set; }

    public bool Rs { get; set; }

    public string? Remarks { get; set; }
}

public class SCBAStatusEntryDto
{
    [Required]
    public Guid SCBAId { get; set; }

    public Guid? VehicleId { get; set; }

    public string? CylinderPressure { get; set; }

    public bool In { get; set; }

    public bool Out { get; set; }

    public bool Rs { get; set; }

    public string? Remarks { get; set; }
}

public class RadioStatusEntryDto
{
    [Required]
    public Guid RadioId { get; set; }

    public bool In { get; set; }

    public bool Out { get; set; }

    public bool Rs { get; set; }

    public string? Remarks { get; set; }
}
