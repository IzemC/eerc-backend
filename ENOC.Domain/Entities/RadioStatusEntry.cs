using System.ComponentModel.DataAnnotations;

namespace ENOC.Domain.Entities;

public class RadioStatusEntry
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid CrewVehicleListingFormId { get; set; }

    [Required]
    public Guid RadioId { get; set; }

    public bool In { get; set; }

    public bool Out { get; set; }

    public bool Rs { get; set; }

    public string? Remarks { get; set; }

    // Navigation properties
    public CrewVehicleListingForm Form { get; set; } = null!;
    public Radio Radio { get; set; } = null!;
}
