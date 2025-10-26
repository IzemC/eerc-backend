using System.ComponentModel.DataAnnotations;
using ENOC.Domain.Enums;

namespace ENOC.Application.DTOs.Tank;

public class UploadTankFileRequest
{
    [Required]
    public Guid TankId { get; set; }

    [Required]
    public string FileName { get; set; } = string.Empty;

    [Required]
    public string ContentType { get; set; } = string.Empty;

    [Required]
    public byte[] FileContent { get; set; } = Array.Empty<byte>();

    [Required]
    public FileType FileType { get; set; }
}
