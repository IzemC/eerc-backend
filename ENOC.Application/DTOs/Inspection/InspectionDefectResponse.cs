namespace ENOC.Application.DTOs.Inspection;

public class InspectionDefectResponse
{
    public Guid Id { get; set; }
    public Guid InspectionId { get; set; }
    public string QuestionId { get; set; } = string.Empty;
    public string QuestionText { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool HasImage { get; set; }
    public string? ImageFileName { get; set; }
    public long? ImageSize { get; set; }
    public DateTime CreatedAt { get; set; }
}
