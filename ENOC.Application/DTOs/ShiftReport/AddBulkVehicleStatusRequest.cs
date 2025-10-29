using System.ComponentModel.DataAnnotations;

namespace ENOC.Application.DTOs.ShiftReport;

public class AddBulkVehicleStatusRequest
{
    [Required]
    public List<VehicleStatusItem> VehicleStatuses { get; set; } = new();
}

public class VehicleStatusItem
{
    [Required]
    public Guid VehicleId { get; set; }

    public string? Description { get; set; }
}
