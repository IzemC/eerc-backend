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

    // Team status endpoints
    /// <summary>
    /// Add a team status entry to a crew vehicle listing
    /// </summary>
    [HttpPost("{id}/team-statuses")]
    public async Task<ActionResult<TeamStatusResponse>> AddTeamStatus(Guid id, [FromBody] AddTeamStatusRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var status = await _crewVehicleListingService.AddTeamStatusAsync(id, request, cancellationToken);
            if (status == null)
            {
                return BadRequest(new { message = "Failed to add team status. Verify that the crew vehicle listing, user, and employee status exist." });
            }

            return CreatedAtAction(nameof(GetTeamStatuses), new { id }, status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding team status to crew vehicle listing {Id}", id);
            return StatusCode(500, new { message = "An error occurred while adding the team status" });
        }
    }

    /// <summary>
    /// Get all team status entries for a crew vehicle listing
    /// </summary>
    [HttpGet("{id}/team-statuses")]
    public async Task<ActionResult<IEnumerable<TeamStatusResponse>>> GetTeamStatuses(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var statuses = await _crewVehicleListingService.GetTeamStatusesByListingIdAsync(id, cancellationToken);
            return Ok(statuses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving team statuses for crew vehicle listing {Id}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving team statuses" });
        }
    }

    /// <summary>
    /// Delete a team status entry
    /// </summary>
    [HttpDelete("team-statuses/{statusId}")]
    public async Task<ActionResult> DeleteTeamStatus(Guid statusId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _crewVehicleListingService.DeleteTeamStatusAsync(statusId, cancellationToken);
            if (!result)
            {
                return NotFound(new { message = "Team status not found" });
            }

            return Ok(new { message = "Team status deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting team status {StatusId}", statusId);
            return StatusCode(500, new { message = "An error occurred while deleting the team status" });
        }
    }

    // Vehicle status endpoints
    /// <summary>
    /// Add a vehicle status entry to a crew vehicle listing
    /// </summary>
    [HttpPost("{id}/vehicle-statuses")]
    public async Task<ActionResult<VehicleStatusResponse>> AddVehicleStatus(Guid id, [FromBody] AddVehicleStatusRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var status = await _crewVehicleListingService.AddVehicleStatusAsync(id, request, cancellationToken);
            if (status == null)
            {
                return BadRequest(new { message = "Failed to add vehicle status. Verify that the crew vehicle listing and vehicle exist." });
            }

            return CreatedAtAction(nameof(GetVehicleStatuses), new { id }, status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding vehicle status to crew vehicle listing {Id}", id);
            return StatusCode(500, new { message = "An error occurred while adding the vehicle status" });
        }
    }

    /// <summary>
    /// Get all vehicle status entries for a crew vehicle listing
    /// </summary>
    [HttpGet("{id}/vehicle-statuses")]
    public async Task<ActionResult<IEnumerable<VehicleStatusResponse>>> GetVehicleStatuses(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var statuses = await _crewVehicleListingService.GetVehicleStatusesByListingIdAsync(id, cancellationToken);
            return Ok(statuses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vehicle statuses for crew vehicle listing {Id}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving vehicle statuses" });
        }
    }

    /// <summary>
    /// Delete a vehicle status entry
    /// </summary>
    [HttpDelete("vehicle-statuses/{statusId}")]
    public async Task<ActionResult> DeleteVehicleStatus(Guid statusId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _crewVehicleListingService.DeleteVehicleStatusAsync(statusId, cancellationToken);
            if (!result)
            {
                return NotFound(new { message = "Vehicle status not found" });
            }

            return Ok(new { message = "Vehicle status deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting vehicle status {StatusId}", statusId);
            return StatusCode(500, new { message = "An error occurred while deleting the vehicle status" });
        }
    }

    // SCBA status endpoints
    /// <summary>
    /// Add an SCBA status entry to a crew vehicle listing
    /// </summary>
    [HttpPost("{id}/scba-statuses")]
    public async Task<ActionResult<SCBAStatusResponse>> AddSCBAStatus(Guid id, [FromBody] AddSCBAStatusRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var status = await _crewVehicleListingService.AddSCBAStatusAsync(id, request, cancellationToken);
            if (status == null)
            {
                return BadRequest(new { message = "Failed to add SCBA status. Verify that the crew vehicle listing and SCBA exist." });
            }

            return CreatedAtAction(nameof(GetSCBAStatuses), new { id }, status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding SCBA status to crew vehicle listing {Id}", id);
            return StatusCode(500, new { message = "An error occurred while adding the SCBA status" });
        }
    }

    /// <summary>
    /// Get all SCBA status entries for a crew vehicle listing
    /// </summary>
    [HttpGet("{id}/scba-statuses")]
    public async Task<ActionResult<IEnumerable<SCBAStatusResponse>>> GetSCBAStatuses(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var statuses = await _crewVehicleListingService.GetSCBAStatusesByListingIdAsync(id, cancellationToken);
            return Ok(statuses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving SCBA statuses for crew vehicle listing {Id}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving SCBA statuses" });
        }
    }

    /// <summary>
    /// Delete an SCBA status entry
    /// </summary>
    [HttpDelete("scba-statuses/{statusId}")]
    public async Task<ActionResult> DeleteSCBAStatus(Guid statusId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _crewVehicleListingService.DeleteSCBAStatusAsync(statusId, cancellationToken);
            if (!result)
            {
                return NotFound(new { message = "SCBA status not found" });
            }

            return Ok(new { message = "SCBA status deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting SCBA status {StatusId}", statusId);
            return StatusCode(500, new { message = "An error occurred while deleting the SCBA status" });
        }
    }

    // Radio status endpoints
    /// <summary>
    /// Add a radio status entry to a crew vehicle listing
    /// </summary>
    [HttpPost("{id}/radio-statuses")]
    public async Task<ActionResult<RadioStatusResponse>> AddRadioStatus(Guid id, [FromBody] AddRadioStatusRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var status = await _crewVehicleListingService.AddRadioStatusAsync(id, request, cancellationToken);
            if (status == null)
            {
                return BadRequest(new { message = "Failed to add radio status. Verify that the crew vehicle listing and radio exist." });
            }

            return CreatedAtAction(nameof(GetRadioStatuses), new { id }, status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding radio status to crew vehicle listing {Id}", id);
            return StatusCode(500, new { message = "An error occurred while adding the radio status" });
        }
    }

    /// <summary>
    /// Get all radio status entries for a crew vehicle listing
    /// </summary>
    [HttpGet("{id}/radio-statuses")]
    public async Task<ActionResult<IEnumerable<RadioStatusResponse>>> GetRadioStatuses(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var statuses = await _crewVehicleListingService.GetRadioStatusesByListingIdAsync(id, cancellationToken);
            return Ok(statuses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving radio statuses for crew vehicle listing {Id}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving radio statuses" });
        }
    }

    /// <summary>
    /// Delete a radio status entry
    /// </summary>
    [HttpDelete("radio-statuses/{statusId}")]
    public async Task<ActionResult> DeleteRadioStatus(Guid statusId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _crewVehicleListingService.DeleteRadioStatusAsync(statusId, cancellationToken);
            if (!result)
            {
                return NotFound(new { message = "Radio status not found" });
            }

            return Ok(new { message = "Radio status deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting radio status {StatusId}", statusId);
            return StatusCode(500, new { message = "An error occurred while deleting the radio status" });
        }
    }
}
