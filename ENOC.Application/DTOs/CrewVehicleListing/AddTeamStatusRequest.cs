using System.ComponentModel.DataAnnotations;

namespace ENOC.Application.DTOs.CrewVehicleListing;

public class AddTeamStatusRequest
{
    [Required]
    public Guid UserId { get; set; }

    public Guid? VehicleId { get; set; }

    [Required]
    public Guid EmployeeStatusId { get; set; }

    public string? Remarks { get; set; }
}
