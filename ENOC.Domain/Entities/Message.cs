using System.ComponentModel.DataAnnotations;

namespace ENOC.Domain.Entities;

public class Message
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Description { get; set; } = string.Empty;

    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    public ICollection<Incident> Incidents { get; set; } = new List<Incident>();
}
