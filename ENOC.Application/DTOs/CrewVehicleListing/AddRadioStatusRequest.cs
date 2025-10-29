using System.ComponentModel.DataAnnotations;

namespace ENOC.Application.DTOs.CrewVehicleListing;

public class AddRadioStatusRequest
{
    [Required]
    public Guid RadioId { get; set; }

    public bool In { get; set; }

    public bool Out { get; set; }

    public bool Rs { get; set; }

    public string? Remarks { get; set; }
}
