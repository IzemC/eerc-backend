using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ENOC.Domain.Entities;

[Table("InspectionDefects")]
public class InspectionDefect
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid InspectionId { get; set; }

    [Required]
    [MaxLength(100)]
    public string QuestionId { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string QuestionText { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    public byte[]? Image { get; set; }

    [MaxLength(255)]
    public string? ImageFileName { get; set; }

    [MaxLength(100)]
    public string? ImageContentType { get; set; }

    public long? ImageSize { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Inspection Inspection { get; set; } = null!;
}
