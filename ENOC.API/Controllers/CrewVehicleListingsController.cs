using ENOC.Application.DTOs.CrewVehicleListing;
using ENOC.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ENOC.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CrewVehicleListingsController : ControllerBase
{
    private readonly ICrewVehicleListingService _crewVehicleListingService;
    private readonly ILogger<CrewVehicleListingsController> _logger;

    public CrewVehicleListingsController(ICrewVehicleListingService crewVehicleListingService, ILogger<CrewVehicleListingsController> logger)
    {
        _crewVehicleListingService = crewVehicleListingService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new crew vehicle listing form
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CrewVehicleListingResponse>> CreateCrewVehicleListing([FromBody] CreateCrewVehicleListingRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var listing = await _crewVehicleListingService.CreateCrewVehicleListingAsync(request, userId, cancellationToken);
            if (listing == null)
            {
                return BadRequest(new { message = "Failed to create crew vehicle listing" });
            }

            _logger.LogInformation("Crew vehicle listing {FormId} created by user {UserId}", listing.CrewVehicleListingFormId, userId);

            return CreatedAtAction(nameof(GetCrewVehicleListingById), new { id = listing.Id }, listing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating crew vehicle listing");
            return StatusCode(500, new { message = "An error occurred while creating the crew vehicle listing" });
        }
    }

    /// <summary>
    /// Get crew vehicle listing by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CrewVehicleListingResponse>> GetCrewVehicleListingById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var listing = await _crewVehicleListingService.GetCrewVehicleListingByIdAsync(id, cancellationToken);
            if (listing == null)
            {
                return NotFound(new { message = "Crew vehicle listing not found" });
            }

            return Ok(listing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving crew vehicle listing {Id}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the crew vehicle listing" });
        }
    }

    /// <summary>
    /// Get all crew vehicle listings with optional filters
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CrewVehicleListingResponse>>> GetAllCrewVehicleListings(
        [FromQuery] Guid? teamId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var listings = await _crewVehicleListingService.GetAllCrewVehicleListingsAsync(teamId, fromDate, toDate, cancellationToken);
            return Ok(listings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving crew vehicle listings");
            return StatusCode(500, new { message = "An error occurred while retrieving crew vehicle listings" });
        }
    }

    /// <summary>
    /// Delete a crew vehicle listing
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCrewVehicleListing(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _crewVehicleListingService.DeleteCrewVehicleListingAsync(id, cancellationToken);
            if (!result)
            {
                return NotFound(new { message = "Crew vehicle listing not found" });
            }

            return Ok(new { message = "Crew vehicle listing deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting crew vehicle listing {Id}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the crew vehicle listing" });
        }
    }
}
