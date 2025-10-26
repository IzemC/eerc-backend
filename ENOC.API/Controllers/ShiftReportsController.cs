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
}
