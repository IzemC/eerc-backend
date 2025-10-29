namespace ENOC.Application.DTOs.ShiftReport;

public class ShiftReportVehicleStatusResponse
{
    public Guid Id { get; set; }
    public Guid ShiftReportFormId { get; set; }
    public Guid VehicleId { get; set; }
    public string VehicleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
