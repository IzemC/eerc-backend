using System.ComponentModel.DataAnnotations;

namespace ENOC.Application.DTOs.CrewVehicleListing;

public class AddSCBAStatusRequest
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
