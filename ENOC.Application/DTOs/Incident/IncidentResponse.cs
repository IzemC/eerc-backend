using ENOC.Domain.Enums;

namespace ENOC.Application.DTOs.Incident;

public class IncidentResponse
{
    public Guid Id { get; set; }
    public string IncidentId { get; set; } = string.Empty;
    public int IncidentCounter { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid IncidentTypeId { get; set; }
    public string IncidentTypeName { get; set; } = string.Empty;
    public string? IncidentTypeImage { get; set; }
    public Guid UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmployeeId { get; set; } = string.Empty;
    public Guid MessageId { get; set; }
    public string MessageDescription { get; set; } = string.Empty;
    public Guid? TankId { get; set; }
    public string? TankName { get; set; }
    public int? TankNumber { get; set; }
    public IncidentStatus Status { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? ReporterName { get; set; }
    public string? ReporterContactDetails { get; set; }
    public string? Team { get; set; }
    public string? CustomMessage { get; set; }
    public string? Action { get; set; }
    public List<IncidentAcknowledgementResponse> Acknowledgements { get; set; } = new();
}

public class IncidentAcknowledgementResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmployeeId { get; set; } = string.Empty;
    public string? ETA { get; set; }
    public string? AcknowledgementStatus { get; set; }
    public DateTime CreatedAt { get; set; }
}
