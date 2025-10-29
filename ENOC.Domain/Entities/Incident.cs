using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ENOC.Domain.Enums;

namespace ENOC.Domain.Entities;

[Table("Incidentes")]
public class Incident
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string IncidentId { get; set; } = string.Empty;

    [Required]
    public int IncidentCounter { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Required]
    public Guid IncidentTypeId { get; set; }

    [Required]
    public Guid UnitId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid MessageId { get; set; }

    public Guid? TankId { get; set; }

    [Required]
    public IncidentStatus Status { get; set; }

    public DateTime? ClosedAt { get; set; }

    public string? ReporterName { get; set; }

    public string? ReporterContactDetails { get; set; }

    public string? Team { get; set; }

    public string? CustomMessage { get; set; }

    public string? Action { get; set; }

    public string? Description { get; set; }

    public DateTime? TimeOfTurnout { get; set; }

    public DateTime? TimeOfArrival { get; set; }

    public DateTime? TimeOfAllClear { get; set; }

    // Navigation properties
    public IncidentType IncidentType { get; set; } = null!;
    public BusinessUnit Unit { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
    public Message Message { get; set; } = null!;
    public Tank? Tank { get; set; }
    public ICollection<IncidentAcknowledgement> Acknowledgements { get; set; } = new List<IncidentAcknowledgement>();
}
