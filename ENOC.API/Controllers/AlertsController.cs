using ENOC.Application.DTOs.Alert;
using ENOC.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ENOC.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AlertsController : ControllerBase
{
    private readonly IAlertService _alertService;
    private readonly ILogger<AlertsController> _logger;

    public AlertsController(IAlertService alertService, ILogger<AlertsController> logger)
    {
        _alertService = alertService;
        _logger = logger;
    }

    /// <summary>
    /// Send alert message to specified teams
    /// </summary>
    [HttpPost("send")]
    public async Task<ActionResult> SendAlert([FromBody] SendAlertRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { message = "Message is required" });
            }

            if (!request.TeamIds.Any())
            {
                return BadRequest(new { message = "At least one team must be specified" });
            }

            var result = await _alertService.SendAlertToTeamsAsync(request, cancellationToken);

            if (!result)
            {
                return BadRequest(new { message = "Failed to send alert to teams" });
            }

            return Ok(new { message = "Alert sent successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending alert");
            return StatusCode(500, new { message = "An error occurred while sending the alert" });
        }
    }
}
