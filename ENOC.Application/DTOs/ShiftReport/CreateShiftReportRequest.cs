using System.ComponentModel.DataAnnotations;

namespace ENOC.Application.DTOs.ShiftReport;

public class CreateShiftReportRequest
{
    [Required]
    public Guid TeamId { get; set; }

    [Required]
    public DateTime From { get; set; }

    [Required]
    public DateTime To { get; set; }

    public Guid? IncidentId { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Details { get; set; }
}
