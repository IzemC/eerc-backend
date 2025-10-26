using ENOC.Application.DTOs.Inspection;
using ENOC.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ENOC.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InspectionsController : ControllerBase
{
    private readonly IInspectionService _inspectionService;
    private readonly ILogger<InspectionsController> _logger;

    public InspectionsController(IInspectionService inspectionService, ILogger<InspectionsController> logger)
    {
        _inspectionService = inspectionService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new vehicle inspection
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<InspectionResponse>> CreateInspection([FromBody] CreateInspectionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var inspection = await _inspectionService.CreateInspectionAsync(request, userId, cancellationToken);
            if (inspection == null)
            {
                return BadRequest(new { message = "Failed to create inspection" });
            }

            _logger.LogInformation("Inspection {InspectionId} created by user {UserId}", inspection.InspectionId, userId);

            return CreatedAtAction(nameof(GetInspectionById), new { id = inspection.Id }, inspection);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating inspection");
            return StatusCode(500, new { message = "An error occurred while creating the inspection" });
        }
    }

    /// <summary>
    /// Get inspection by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<InspectionResponse>> GetInspectionById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var inspection = await _inspectionService.GetInspectionByIdAsync(id, cancellationToken);
            if (inspection == null)
            {
                return NotFound(new { message = "Inspection not found" });
            }

            return Ok(inspection);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving inspection {Id}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the inspection" });
        }
    }

    /// <summary>
    /// Get all inspections with optional filters
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<InspectionResponse>>> GetAllInspections(
        [FromQuery] Guid? vehicleId = null,
        [FromQuery] bool? isDefected = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var inspections = await _inspectionService.GetAllInspectionsAsync(vehicleId, isDefected, fromDate, toDate, cancellationToken);
            return Ok(inspections);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving inspections");
            return StatusCode(500, new { message = "An error occurred while retrieving inspections" });
        }
    }

    /// <summary>
    /// Delete an inspection
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteInspection(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _inspectionService.DeleteInspectionAsync(id, cancellationToken);
            if (!result)
            {
                return NotFound(new { message = "Inspection not found" });
            }

            return Ok(new { message = "Inspection deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting inspection {Id}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the inspection" });
        }
    }
}
