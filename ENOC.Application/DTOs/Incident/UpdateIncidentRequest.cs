using ENOC.Domain.Enums;

namespace ENOC.Application.DTOs.Incident;

public class UpdateIncidentRequest
{
    public Guid? IncidentTypeId { get; set; }

    public Guid? UnitId { get; set; }

    public Guid? MessageId { get; set; }

    public Guid? TankId { get; set; }

    public IncidentStatus? Status { get; set; }

    public string? ReporterName { get; set; }

    public string? ReporterContactDetails { get; set; }

    public string? Team { get; set; }

    public string? CustomMessage { get; set; }

    public string? Action { get; set; }
}
