namespace ENOC.Application.DTOs.DeviceToken;

public class DeviceTokenResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string DeviceToken { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string? DeviceName { get; set; }
    public bool IsActive { get; set; }
    public DateTime RegisteredAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
}
