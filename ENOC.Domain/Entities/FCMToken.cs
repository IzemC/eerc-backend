using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ENOC.Domain.Enums;

namespace ENOC.Domain.Entities;

[Table("FCMTokens")]
public class FCMToken
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public DeviceType DeviceType { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
}
