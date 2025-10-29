using ENOC.Application.DTOs.ShiftReport;
using ENOC.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ENOC.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ShiftReportsController : ControllerBase
{
    private readonly IShiftReportService _shiftReportService;
    private readonly ILogger<ShiftReportsController> _logger;

    public ShiftReportsController(IShiftReportService shiftReportService, ILogger<ShiftReportsController> logger)
    {
        _shiftReportService = shiftReportService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new shift report
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ShiftReportResponse>> CreateShiftReport([FromBody] CreateShiftReportRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var report = await _shiftReportService.CreateShiftReportAsync(request, userId, cancellationToken);
            if (report == null)
            {
                return BadRequest(new { message = "Failed to create shift report" });
            }

            _logger.LogInformation("Shift report {ShiftFormId} created by user {UserId}", report.ShiftFormId, userId);

            return CreatedAtAction(nameof(GetShiftReportById), new { id = report.Id }, report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating shift report");
            return StatusCode(500, new { message = "An error occurred while creating the shift report" });
        }
    }

    /// <summary>
    /// Get shift report by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ShiftReportResponse>> GetShiftReportById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var report = await _shiftReportService.GetShiftReportByIdAsync(id, cancellationToken);
            if (report == null)
            {
                return NotFound(new { message = "Shift report not found" });
            }

            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shift report {Id}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the shift report" });
        }
    }

    /// <summary>
    /// Get all shift reports with optional filters
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ShiftReportResponse>>> GetAllShiftReports(
        [FromQuery] Guid? teamId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var reports = await _shiftReportService.GetAllShiftReportsAsync(teamId, fromDate, toDate, cancellationToken);
            return Ok(reports);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shift reports");
            return StatusCode(500, new { message = "An error occurred while retrieving shift reports" });
        }
    }

    /// <summary>
    /// Delete a shift report
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteShiftReport(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _shiftReportService.DeleteShiftReportAsync(id, cancellationToken);
            if (!result)
            {
                return NotFound(new { message = "Shift report not found" });
            }

            return Ok(new { message = "Shift report deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting shift report {Id}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the shift report" });
        }
    }

    /// <summary>
    /// Add a vehicle status to a shift report
    /// </summary>
    [HttpPost("{id}/vehicle-statuses")]
    public async Task<ActionResult<ShiftReportVehicleStatusResponse>> AddVehicleStatus(
        Guid id,
        [FromBody] AddShiftReportVehicleStatusRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var status = await _shiftReportService.AddVehicleStatusAsync(id, request.VehicleId, request.Description, cancellationToken);
            if (status == null)
            {
                return BadRequest(new { message = "Failed to add vehicle status. Shift report or vehicle not found." });
            }

            _logger.LogInformation("Vehicle status added to shift report {ShiftReportId}", id);

            return CreatedAtAction(nameof(GetVehicleStatuses), new { id }, status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding vehicle status to shift report {Id}", id);
            return StatusCode(500, new { message = "An error occurred while adding the vehicle status" });
        }
    }

    /// <summary>
    /// Add multiple vehicle statuses to a shift report at once
    /// </summary>
    [HttpPost("{id}/vehicle-statuses/bulk")]
    public async Task<ActionResult<List<ShiftReportVehicleStatusResponse>>> AddBulkVehicleStatuses(
        Guid id,
        [FromBody] AddBulkVehicleStatusRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var statuses = await _shiftReportService.AddBulkVehicleStatusAsync(id, request, cancellationToken);
            if (!statuses.Any())
            {
                return BadRequest(new { message = "Failed to add vehicle statuses. Shift report not found or no valid vehicles provided." });
            }

            _logger.LogInformation("Added {Count} vehicle statuses to shift report {ShiftReportId}", statuses.Count, id);

            return Ok(statuses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding bulk vehicle statuses to shift report {Id}", id);
            return StatusCode(500, new { message = "An error occurred while adding the vehicle statuses" });
        }
    }

    /// <summary>
    /// Get all vehicle statuses for a shift report
    /// </summary>
    [HttpGet("{id}/vehicle-statuses")]
    public async Task<ActionResult<IEnumerable<ShiftReportVehicleStatusResponse>>> GetVehicleStatuses(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var statuses = await _shiftReportService.GetVehicleStatusesByShiftReportIdAsync(id, cancellationToken);
            return Ok(statuses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vehicle statuses for shift report {Id}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving vehicle statuses" });
        }
    }

    /// <summary>
    /// Delete a vehicle status
    /// </summary>
    [HttpDelete("vehicle-statuses/{statusId}")]
    public async Task<ActionResult> DeleteVehicleStatus(Guid statusId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _shiftReportService.DeleteVehicleStatusAsync(statusId, cancellationToken);
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

    /// <summary>
    /// Update activities for a shift report
    /// </summary>
    [HttpPut("{id}/activities")]
    public async Task<ActionResult<ShiftReportResponse>> UpdateActivities(
        Guid id,
        [FromBody] UpdateActivitiesRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var report = await _shiftReportService.UpdateActivitiesAsync(id, request.Activities, cancellationToken);
            if (report == null)
            {
                return NotFound(new { message = "Shift report not found" });
            }

            _logger.LogInformation("Activities updated for shift report {ShiftReportId}", id);

            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating activities for shift report {Id}", id);
            return StatusCode(500, new { message = "An error occurred while updating activities" });
        }
    }
}

public record AddShiftReportVehicleStatusRequest(Guid VehicleId, string? Description);
public record UpdateActivitiesRequest(string Activities);
