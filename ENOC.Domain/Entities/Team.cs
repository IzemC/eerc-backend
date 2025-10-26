using System.ComponentModel.DataAnnotations;

namespace ENOC.Domain.Entities;

public class Team
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Color { get; set; }

    public string? ColorName { get; set; }

    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    public ICollection<ShiftReportForm> ShiftReportForms { get; set; } = new List<ShiftReportForm>();
    public ICollection<CrewVehicleListingForm> CrewVehicleListingForms { get; set; } = new List<CrewVehicleListingForm>();
}
