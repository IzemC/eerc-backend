using System.ComponentModel.DataAnnotations;

namespace ENOC.Application.DTOs.DeviceToken;

public class RegisterDeviceTokenRequest
{
    [Required]
    public string DeviceToken { get; set; } = string.Empty;

    [Required]
    public string DeviceType { get; set; } = string.Empty; // "iOS", "Android", "Web"

    public string? DeviceName { get; set; }
}
