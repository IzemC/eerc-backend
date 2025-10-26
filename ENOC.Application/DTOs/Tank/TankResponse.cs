namespace ENOC.Application.DTOs.Tank;

public class TankResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TankNumber { get; set; }
    public Guid BusinessUnitId { get; set; }
    public string BusinessUnitName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? ERG { get; set; }
    public List<TankFileResponse> Files { get; set; } = new();
}
