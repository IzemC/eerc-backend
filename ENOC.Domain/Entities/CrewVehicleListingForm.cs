using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ENOC.Domain.Entities;

[Table("CrewVehiclesListingForm")]
public class CrewVehicleListingForm
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string CrewVehicleListingFormId { get; set; } = string.Empty;

    [Required]
    public int CrewVehicleListingFormCounter { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid TeamId { get; set; }

    [Required]
    public DateTime From { get; set; }

    [Required]
    public DateTime To { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Details { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
    public Team Team { get; set; } = null!;
    public ICollection<TeamStatusEntry> TeamStatuses { get; set; } = new List<TeamStatusEntry>();
    public ICollection<FireVehicleStatusEntry> VehicleStatuses { get; set; } = new List<FireVehicleStatusEntry>();
    public ICollection<SCBAStatusEntry> SCBAStatuses { get; set; } = new List<SCBAStatusEntry>();
    public ICollection<RadioStatusEntry> RadioStatuses { get; set; } = new List<RadioStatusEntry>();
}
