using System.ComponentModel.DataAnnotations;

namespace ENOC.Domain.Entities;

public class IncidentType
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Image { get; set; }

    public bool AlarmPA { get; set; } = false;

    public string? AlarmVoice { get; set; }

    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    public ICollection<Incident> Incidents { get; set; } = new List<Incident>();
}
