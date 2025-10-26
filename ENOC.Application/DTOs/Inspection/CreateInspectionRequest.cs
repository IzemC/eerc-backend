using System.ComponentModel.DataAnnotations;

namespace ENOC.Application.DTOs.Inspection;

public class CreateInspectionRequest
{
    [Required]
    public Guid VehicleId { get; set; }

    [Required]
    public string Answers { get; set; } = string.Empty; // JSON string

    [Required]
    public bool IsDefected { get; set; }
}
