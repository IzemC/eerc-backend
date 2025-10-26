using System.ComponentModel.DataAnnotations;

namespace ENOC.Domain.Entities;

public class Radio
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Location { get; set; }

    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    public ICollection<RadioStatusEntry> RadioStatusEntries { get; set; } = new List<RadioStatusEntry>();
}
