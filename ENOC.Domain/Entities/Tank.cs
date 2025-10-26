using System.ComponentModel.DataAnnotations;

namespace ENOC.Domain.Entities;

public class Tank
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public int TankNumber { get; set; }

    [Required]
    public Guid BusinessUnitId { get; set; }

    public string? Location { get; set; }

    public string? ERG { get; set; }

    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    public BusinessUnit BusinessUnit { get; set; } = null!;
    public ICollection<TankFile> Files { get; set; } = new List<TankFile>();
    public ICollection<Incident> Incidents { get; set; } = new List<Incident>();
}
