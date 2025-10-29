namespace ENOC.Application.DTOs.Inspection;

public class InspectionResponse
{
    public Guid Id { get; set; }
    public string InspectionId { get; set; } = string.Empty;
    public int InspectionCounter { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmployeeId { get; set; } = string.Empty;
    public Guid VehicleId { get; set; }
    public string VehicleName { get; set; } = string.Empty;
    public string? VehiclePlateNumber { get; set; }
    public string Answers { get; set; } = string.Empty;
    public bool IsDefected { get; set; }
    public byte[]? UserSignature { get; set; }
    public string? UserSignatureContentType { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<InspectionDefectResponse> Defects { get; set; } = new();
}
