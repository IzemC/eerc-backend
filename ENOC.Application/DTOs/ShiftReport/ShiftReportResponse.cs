namespace ENOC.Application.DTOs.ShiftReport;

public class ShiftReportResponse
{
    public Guid Id { get; set; }
    public string ShiftFormId { get; set; } = string.Empty;
    public int ShiftFormCounter { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmployeeId { get; set; } = string.Empty;
    public Guid TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public Guid? IncidentId { get; set; }
    public string? IncidentNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; }
}
