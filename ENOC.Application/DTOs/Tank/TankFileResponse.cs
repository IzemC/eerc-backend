using ENOC.Domain.Enums;

namespace ENOC.Application.DTOs.Tank;

public class TankFileResponse
{
    public Guid Id { get; set; }
    public Guid TankId { get; set; }
    public string TankName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public FileType FileType { get; set; }
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
    public string ContentType { get; set; } = string.Empty;
}
