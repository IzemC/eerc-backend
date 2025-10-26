using System.ComponentModel.DataAnnotations;

namespace ENOC.Domain.Entities;

public class TeamStatusEntry
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid CrewVehicleListingFormId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    public Guid? VehicleId { get; set; }

    [Required]
    public Guid EmployeeStatusId { get; set; }

    public string? Remarks { get; set; }

    // Navigation properties
    public CrewVehicleListingForm Form { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
    public Vehicle? Vehicle { get; set; }
    public EmployeeStatus EmployeeStatus { get; set; } = null!;
}
