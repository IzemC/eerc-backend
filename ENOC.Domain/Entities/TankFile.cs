using System.ComponentModel.DataAnnotations;
using ENOC.Domain.Enums;

namespace ENOC.Domain.Entities;

public class TankFile
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid TankId { get; set; }

    [Required]
    public string FileName { get; set; } = string.Empty;

    [Required]
    public string FileExtension { get; set; } = string.Empty;

    [Required]
    public byte[] FileContent { get; set; } = Array.Empty<byte>();

    [Required]
    public FileType FileType { get; set; }

    [Required]
    public long FileSize { get; set; }

    [Required]
    public DateTime UploadedAt { get; set; } = DateTime.Now;

    [Required]
    public string ContentType { get; set; } = string.Empty;

    // Navigation properties
    public Tank Tank { get; set; } = null!;
}
