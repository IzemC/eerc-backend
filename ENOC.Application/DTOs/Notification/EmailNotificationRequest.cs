using System.ComponentModel.DataAnnotations;

namespace ENOC.Application.DTOs.Notification;

public class EmailNotificationRequest
{
    [Required]
    [EmailAddress]
    public string To { get; set; } = string.Empty;

    public List<string> Cc { get; set; } = new();

    public List<string> Bcc { get; set; } = new();

    [Required]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;

    public bool IsHtml { get; set; } = true;

    public List<EmailAttachment> Attachments { get; set; } = new();
}

public class EmailAttachment
{
    [Required]
    public string FileName { get; set; } = string.Empty;

    [Required]
    public byte[] Content { get; set; } = Array.Empty<byte>();

    [Required]
    public string ContentType { get; set; } = string.Empty;
}
