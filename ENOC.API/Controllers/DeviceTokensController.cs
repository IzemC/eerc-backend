using ENOC.Application.DTOs.DeviceToken;
using ENOC.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ENOC.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DeviceTokensController : ControllerBase
{
    private readonly IDeviceTokenService _deviceTokenService;
    private readonly ILogger<DeviceTokensController> _logger;

    public DeviceTokensController(IDeviceTokenService deviceTokenService, ILogger<DeviceTokensController> logger)
    {
        _deviceTokenService = deviceTokenService;
        _logger = logger;
    }

    /// <summary>
    /// Register a device token for push notifications
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<DeviceTokenResponse>> RegisterDeviceToken([FromBody] RegisterDeviceTokenRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var deviceToken = await _deviceTokenService.RegisterDeviceTokenAsync(userId, request, cancellationToken);

            _logger.LogInformation("Device token registered for user {UserId}", userId);

            return Ok(deviceToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering device token");
            return StatusCode(500, new { message = "An error occurred while registering the device token" });
        }
    }

    /// <summary>
    /// Get all device tokens for the current user
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DeviceTokenResponse>>> GetMyDeviceTokens(CancellationToken cancellationToken)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var tokens = await _deviceTokenService.GetUserDeviceTokensAsync(userId, cancellationToken);

            return Ok(tokens);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving device tokens");
            return StatusCode(500, new { message = "An error occurred while retrieving device tokens" });
        }
    }

    /// <summary>
    /// Deactivate a device token
    /// </summary>
    [HttpPost("deactivate")]
    public async Task<ActionResult> DeactivateDeviceToken([FromBody] string deviceToken, CancellationToken cancellationToken)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var result = await _deviceTokenService.DeactivateDeviceTokenAsync(userId, deviceToken, cancellationToken);
            if (!result)
            {
                return NotFound(new { message = "Device token not found" });
            }

            return Ok(new { message = "Device token deactivated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating device token");
            return StatusCode(500, new { message = "An error occurred while deactivating the device token" });
        }
    }

    /// <summary>
    /// Delete a device token
    /// </summary>
    [HttpDelete("{tokenId}")]
    public async Task<ActionResult> DeleteDeviceToken(Guid tokenId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _deviceTokenService.DeleteDeviceTokenAsync(tokenId, cancellationToken);
            if (!result)
            {
                return NotFound(new { message = "Device token not found" });
            }

            return Ok(new { message = "Device token deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting device token {TokenId}", tokenId);
            return StatusCode(500, new { message = "An error occurred while deleting the device token" });
        }
    }
}
