using System.ComponentModel.DataAnnotations;

namespace ENOC.Domain.Entities;

public class BusinessUnit
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    public ICollection<Tank> Tanks { get; set; } = new List<Tank>();
    public ICollection<Incident> Incidents { get; set; } = new List<Incident>();
}
