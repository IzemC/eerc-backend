using System.ComponentModel.DataAnnotations;

namespace ENOC.Application.DTOs.CrewVehicleListing;

public class AddVehicleStatusRequest
{
    [Required]
    public Guid VehicleId { get; set; }

    public bool In { get; set; }

    public bool Out { get; set; }

    public bool Rs { get; set; }

    public string? Remarks { get; set; }
}
