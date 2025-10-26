using ENOC.Application.DTOs.Map;
using ENOC.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ENOC.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MapsController : ControllerBase
{
    private readonly IMapService _mapService;
    private readonly ILogger<MapsController> _logger;

    public MapsController(IMapService mapService, ILogger<MapsController> logger)
    {
        _mapService = mapService;
        _logger = logger;
    }

    /// <summary>
    /// Get map data with tank locations and incident markers
    /// Tanks with open incidents are marked for red display
    /// </summary>
    [HttpGet("data")]
    public async Task<ActionResult<MapDataResponse>> GetMapData([FromQuery] Guid? businessUnitId, CancellationToken cancellationToken)
    {
        try
        {
            var mapData = await _mapService.GetMapDataAsync(businessUnitId, cancellationToken);
            return Ok(mapData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving map data");
            return StatusCode(500, new { message = "An error occurred while retrieving map data" });
        }
    }
}
