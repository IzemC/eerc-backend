using ENOC.Application.DTOs.Incident;
using ENOC.Application.Interfaces;
using ENOC.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ENOC.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IncidentsController : ControllerBase
{
    private readonly IIncidentService _incidentService;
    private readonly ILogger<IncidentsController> _logger;

    public IncidentsController(IIncidentService incidentService, ILogger<IncidentsController> logger)
    {
        _incidentService = incidentService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new incident
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<IncidentResponse>> CreateIncident([FromBody] CreateIncidentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var incident = await _incidentService.CreateIncidentAsync(request, userId, cancellationToken);
            if (incident == null)
            {
                return BadRequest(new { message = "Failed to create incident" });
            }

            _logger.LogInformation("Incident {IncidentId} created by user {UserId}", incident.IncidentId, userId);

            return CreatedAtAction(nameof(GetIncidentById), new { id = incident.Id }, incident);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating incident");
            return StatusCode(500, new { message = "An error occurred while creating the incident" });
        }
    }

    /// <summary>
    /// Get incident by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<IncidentResponse>> GetIncidentById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var incident = await _incidentService.GetIncidentByIdAsync(id, cancellationToken);
            if (incident == null)
            {
                return NotFound(new { message = "Incident not found" });
            }

            return Ok(incident);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving incident {IncidentId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the incident" });
        }
    }

    /// <summary>
    /// Get all incidents with optional filters
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<IncidentResponse>>> GetAllIncidents(
        [FromQuery] IncidentStatus? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var incidents = await _incidentService.GetAllIncidentsAsync(status, fromDate, toDate, cancellationToken);
            return Ok(incidents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving incidents");
            return StatusCode(500, new { message = "An error occurred while retrieving incidents" });
        }
    }

    /// <summary>
    /// Update an existing incident
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<IncidentResponse>> UpdateIncident(Guid id, [FromBody] UpdateIncidentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var incident = await _incidentService.UpdateIncidentAsync(id, request, cancellationToken);
            if (incident == null)
            {
                return NotFound(new { message = "Incident not found" });
            }

            _logger.LogInformation("Incident {IncidentId} updated", incident.IncidentId);

            return Ok(incident);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating incident {IncidentId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the incident" });
        }
    }

    /// <summary>
    /// Close an incident
    /// </summary>
    [HttpPost("{id}/close")]
    public async Task<ActionResult> CloseIncident(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var result = await _incidentService.CloseIncidentAsync(id, userId, cancellationToken);
            if (!result)
            {
                return NotFound(new { message = "Incident not found" });
            }

            _logger.LogInformation("Incident {IncidentId} closed by user {UserId}", id, userId);

            return Ok(new { message = "Incident closed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing incident {IncidentId}", id);
            return StatusCode(500, new { message = "An error occurred while closing the incident" });
        }
    }

    /// <summary>
    /// Acknowledge an incident
    /// </summary>
    [HttpPost("{id}/acknowledge")]
    public async Task<ActionResult> AcknowledgeIncident(Guid id, [FromBody] AcknowledgeIncidentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var result = await _incidentService.AcknowledgeIncidentAsync(id, userId, request, cancellationToken);
            if (!result)
            {
                return NotFound(new { message = "Incident not found" });
            }

            _logger.LogInformation("Incident {IncidentId} acknowledged by user {UserId}", id, userId);

            return Ok(new { message = "Incident acknowledged successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acknowledging incident {IncidentId}", id);
            return StatusCode(500, new { message = "An error occurred while acknowledging the incident" });
        }
    }

    /// <summary>
    /// Get all acknowledgements for an incident
    /// </summary>
    [HttpGet("{id}/acknowledgements")]
    public async Task<ActionResult<IEnumerable<IncidentAcknowledgementResponse>>> GetIncidentAcknowledgements(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var acknowledgements = await _incidentService.GetIncidentAcknowledgementsAsync(id, cancellationToken);
            return Ok(acknowledgements);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving acknowledgements for incident {IncidentId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving acknowledgements" });
        }
    }
}
