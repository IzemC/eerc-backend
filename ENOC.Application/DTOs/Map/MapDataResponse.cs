namespace ENOC.Application.DTOs.Map;

public class MapDataResponse
{
    public List<TankMapMarker> Tanks { get; set; } = new();
    public int TotalTanks { get; set; }
    public int TanksWithOpenIncidents { get; set; }
}
