using System.ComponentModel.DataAnnotations;

namespace ENOC.Domain.Entities;

public class SCBAStatusEntry
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid CrewVehicleListingFormId { get; set; }

    [Required]
    public Guid SCBAId { get; set; }

    public Guid? VehicleId { get; set; }

    public string? CylinderPressure { get; set; }

    public bool In { get; set; }

    public bool Out { get; set; }

    public bool Rs { get; set; }

    public string? Remarks { get; set; }

    // Navigation properties
    public CrewVehicleListingForm Form { get; set; } = null!;
    public SCBA SCBA { get; set; } = null!;
    public Vehicle? Vehicle { get; set; }
}
