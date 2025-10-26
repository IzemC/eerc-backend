using System.ComponentModel.DataAnnotations;

namespace ENOC.Application.DTOs.Notification;

public class PushNotificationRequest
{
    [Required]
    public string DeviceToken { get; set; } = string.Empty;

    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;

    public Dictionary<string, string> Data { get; set; } = new();

    public string? ImageUrl { get; set; }

    public string? Sound { get; set; } = "default";
}

public class PushNotificationToUserRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;

    public Dictionary<string, string> Data { get; set; } = new();

    public string? ImageUrl { get; set; }
}
