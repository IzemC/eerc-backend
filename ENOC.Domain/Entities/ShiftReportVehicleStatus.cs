using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ENOC.Domain.Entities;

[Table("ShiftReportVehicleStatuses")]
public class ShiftReportVehicleStatus
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid ShiftReportFormId { get; set; }

    [Required]
    public Guid VehicleId { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ShiftReportForm ShiftReportForm { get; set; } = null!;
    public Vehicle Vehicle { get; set; } = null!;
}
