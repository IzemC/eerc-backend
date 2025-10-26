using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ENOC.Domain.Entities;

[Table("Inspections")]
public class Inspection
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string InspectionId { get; set; } = string.Empty;

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid VehicleId { get; set; }

    [Required]
    public int InspectionCounter { get; set; }

    [Required]
    public string Answers { get; set; } = string.Empty;

    [Required]
    public bool IsDefected { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
    public Vehicle Vehicle { get; set; } = null!;
}
