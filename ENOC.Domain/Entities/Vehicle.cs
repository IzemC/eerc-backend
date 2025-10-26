using System.ComponentModel.DataAnnotations;

namespace ENOC.Domain.Entities;

public class Vehicle
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? PlateNumber { get; set; }

    public string? Image { get; set; }

    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    public ICollection<Inspection> Inspections { get; set; } = new List<Inspection>();
    public ICollection<TeamStatusEntry> TeamStatusEntries { get; set; } = new List<TeamStatusEntry>();
    public ICollection<FireVehicleStatusEntry> FireVehicleStatusEntries { get; set; } = new List<FireVehicleStatusEntry>();
    public ICollection<SCBAStatusEntry> SCBAStatusEntries { get; set; } = new List<SCBAStatusEntry>();
}
