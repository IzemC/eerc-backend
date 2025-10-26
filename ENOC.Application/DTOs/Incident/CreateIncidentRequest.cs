using System.ComponentModel.DataAnnotations;

namespace ENOC.Application.DTOs.Incident;

public class CreateIncidentRequest
{
    [Required]
    public Guid IncidentTypeId { get; set; }

    [Required]
    public Guid UnitId { get; set; }

    [Required]
    public Guid MessageId { get; set; }

    public Guid? TankId { get; set; }

    public string? ReporterName { get; set; }

    public string? ReporterContactDetails { get; set; }

    public string? Team { get; set; }

    public string? CustomMessage { get; set; }

    public string? Action { get; set; }
}
