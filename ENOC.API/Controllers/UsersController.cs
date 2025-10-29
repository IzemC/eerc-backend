using ENOC.Application.DTOs.User;
using ENOC.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ENOC.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Get all users
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetAllUsers(CancellationToken cancellationToken)
    {
        try
        {
            var users = await _userService.GetAllUsersAsync(cancellationToken);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return StatusCode(500, new { message = "An error occurred while retrieving users" });
        }
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponse>> GetUserById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id, cancellationToken);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the user" });
        }
    }

    /// <summary>
    /// Create new user
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UserResponse>> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userService.CreateUserAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to create user");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, new { message = "An error occurred while creating the user" });
        }
    }

    /// <summary>
    /// Update user
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<UserResponse>> UpdateUser(Guid id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userService.UpdateUserAsync(id, request, cancellationToken);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to update user {UserId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the user" });
        }
    }

    /// <summary>
    /// Delete user (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUser(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _userService.DeleteUserAsync(id, cancellationToken);
            if (!result)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(new { message = "User deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the user" });
        }
    }

    /// <summary>
    /// Upload or update user signature
    /// </summary>
    [HttpPut("{id}/signature")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult> UpdateUserSignature(Guid id, IFormFile signature, CancellationToken cancellationToken)
    {
        try
        {
            // Validate signature file
            if (signature == null || signature.Length == 0)
            {
                return BadRequest(new { message = "Signature image is required" });
            }

            // Validate file type
            var allowedContentTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/bmp", "image/webp" };
            if (!allowedContentTypes.Contains(signature.ContentType.ToLower()))
            {
                return BadRequest(new { message = $"File type {signature.ContentType} is not supported. Only images are allowed." });
            }

            // Validate file size (max 5MB for signatures)
            const long maxFileSize = 5 * 1024 * 1024;
            if (signature.Length > maxFileSize)
            {
                return BadRequest(new { message = "Signature image size exceeds 5MB limit" });
            }

            // Read signature content
            byte[] signatureContent;
            using (var memoryStream = new MemoryStream())
            {
                await signature.CopyToAsync(memoryStream, cancellationToken);
                signatureContent = memoryStream.ToArray();
            }

            var result = await _userService.UpdateUserSignatureAsync(id, signatureContent, signature.ContentType, cancellationToken);
            if (!result)
            {
                return NotFound(new { message = "User not found" });
            }

            _logger.LogInformation("Signature updated for user {UserId}", id);

            return Ok(new { message = "Signature updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating signature for user {UserId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the signature" });
        }
    }

    /// <summary>
    /// Download user signature
    /// </summary>
    [HttpGet("{id}/signature")]
    public async Task<IActionResult> GetUserSignature(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var signatureData = await _userService.GetUserSignatureAsync(id, cancellationToken);
            if (signatureData == null)
            {
                return NotFound(new { message = "User signature not found" });
            }

            var (content, contentType) = signatureData.Value;

            _logger.LogInformation("Signature downloaded for user {UserId}", id);

            return File(content, contentType, $"user-{id}-signature.png");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading signature for user {UserId}", id);
            return StatusCode(500, new { message = "An error occurred while downloading the signature" });
        }
    }
}
