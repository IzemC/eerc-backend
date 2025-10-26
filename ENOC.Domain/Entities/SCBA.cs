using System.ComponentModel.DataAnnotations;

namespace ENOC.Domain.Entities;

public class SCBA
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    public ICollection<SCBAStatusEntry> SCBAStatusEntries { get; set; } = new List<SCBAStatusEntry>();
}
