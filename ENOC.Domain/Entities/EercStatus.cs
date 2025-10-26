using System.ComponentModel.DataAnnotations;

namespace ENOC.Domain.Entities;

public class EercStatus
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Symbol { get; set; }

    public bool IsDeleted { get; set; } = false;
}
