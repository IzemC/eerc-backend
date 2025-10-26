namespace ENOC.Dashboard.Models;

public class MapData
{
    public List<TankMapMarker> Tanks { get; set; } = new();
    public DateTime Timestamp { get; set; }
}

public class TankMapMarker
{
    public Guid TankId { get; set; }
    public string TankName { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public bool HasOpenIncident { get; set; }
    public int OpenIncidentCount { get; set; }
    public List<string>? PolygonCoordinates { get; set; }
}
