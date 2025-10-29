using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ENOC.Domain.Entities;

[Table("ShiftsReportForm")]
public class ShiftReportForm
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string ShiftFormId { get; set; } = string.Empty;

    [Required]
    public int ShiftFormCounter { get; set; }

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

    public string? Activities { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
    public Team Team { get; set; } = null!;
    public ICollection<ShiftReportVehicleStatus> VehicleStatuses { get; set; } = new List<ShiftReportVehicleStatus>();
}
