using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ENOC.Domain.Entities;

[Table("IncidentsAcknowledgement")]
public class IncidentAcknowledgement
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid IncidentId { get; set; }

    public string? ETA { get; set; }

    public string? AcknowledgementStatus { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
    public Incident Incident { get; set; } = null!;
}
