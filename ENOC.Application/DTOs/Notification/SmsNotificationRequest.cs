using System.ComponentModel.DataAnnotations;

namespace ENOC.Application.DTOs.Notification;

public class SmsNotificationRequest
{
    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Message { get; set; } = string.Empty;
}
