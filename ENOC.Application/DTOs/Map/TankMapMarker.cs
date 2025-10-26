namespace ENOC.Application.DTOs.Map;

public class TankMapMarker
{
    public Guid TankId { get; set; }
    public string TankName { get; set; } = string.Empty;
    public string TankNumber { get; set; } = string.Empty;
    public string BusinessUnit { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public bool HasOpenIncident { get; set; }
    public int OpenIncidentCount { get; set; }
    public List<string>? PolygonCoordinates { get; set; }
}
