namespace ENOC.Domain.Entities;

/// <summary>
/// Stores device tokens for push notifications (FCM/APNS)
/// </summary>
public class UserDeviceToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string DeviceToken { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty; // "iOS", "Android", "Web"
    public string? DeviceName { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime RegisteredAt { get; set; }
    public DateTime? LastUsedAt { get; set; }

    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
}
