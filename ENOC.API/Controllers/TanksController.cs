using ENOC.Application.DTOs.Tank;
using ENOC.Application.Interfaces;
using ENOC.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ENOC.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TanksController : ControllerBase
{
    private readonly ITankService _tankService;
    private readonly ILogger<TanksController> _logger;

    public TanksController(ITankService tankService, ILogger<TanksController> logger)
    {
        _tankService = tankService;
        _logger = logger;
    }

    /// <summary>
    /// Get tank by ID with all files
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TankResponse>> GetTankById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var tank = await _tankService.GetTankByIdAsync(id, cancellationToken);
            if (tank == null)
            {
                return NotFound(new { message = "Tank not found" });
            }

            return Ok(tank);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tank {Id}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the tank" });
        }
    }

    /// <summary>
    /// Get all tanks with optional business unit filter
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TankResponse>>> GetAllTanks(
        [FromQuery] Guid? businessUnitId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tanks = await _tankService.GetAllTanksAsync(businessUnitId, cancellationToken);
            return Ok(tanks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tanks");
            return StatusCode(500, new { message = "An error occurred while retrieving tanks" });
        }
    }

    /// <summary>
    /// Search tanks by name or tank number
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<TankResponse>>> SearchTanks(
        [FromQuery] string query,
        [FromQuery] Guid? businessUnitId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(new { message = "Search query is required" });
            }

            var tanks = await _tankService.SearchTanksAsync(query, businessUnitId, cancellationToken);
            return Ok(tanks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching tanks with query: {Query}", query);
            return StatusCode(500, new { message = "An error occurred while searching tanks" });
        }
    }

    /// <summary>
    /// Upload a file for a tank
    /// </summary>
    [HttpPost("{tankId}/files")]
    public async Task<ActionResult<TankFileResponse>> UploadFile(
        Guid tankId,
        [FromForm] IFormFile file,
        [FromForm] FileType fileType,
        CancellationToken cancellationToken)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file provided" });
            }

            // Validate file type
            var allowedContentTypes = new[] { "application/pdf", "image/jpeg", "image/jpg", "image/png", "image/gif", "image/bmp", "image/webp" };
            if (!allowedContentTypes.Contains(file.ContentType.ToLower()))
            {
                return BadRequest(new { message = $"File type {file.ContentType} is not supported. Allowed types: PDF and images." });
            }

            // Validate file size (max 50MB)
            const long maxFileSize = 50 * 1024 * 1024;
            if (file.Length > maxFileSize)
            {
                return BadRequest(new { message = "File size exceeds 50MB limit" });
            }

            // Read file content
            byte[] fileContent;
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream, cancellationToken);
                fileContent = memoryStream.ToArray();
            }

            var request = new UploadTankFileRequest
            {
                TankId = tankId,
                FileName = file.FileName,
                ContentType = file.ContentType,
                FileContent = fileContent,
                FileType = fileType
            };

            var uploadedFile = await _tankService.UploadTankFileAsync(request, cancellationToken);
            if (uploadedFile == null)
            {
                return NotFound(new { message = "Tank not found" });
            }

            _logger.LogInformation("File {FileName} uploaded for tank {TankId}", file.FileName, tankId);

            return CreatedAtAction(nameof(GetTankFiles), new { tankId }, uploadedFile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file for tank {TankId}", tankId);
            return StatusCode(500, new { message = "An error occurred while uploading the file" });
        }
    }

    /// <summary>
    /// Download a tank file
    /// </summary>
    [HttpGet("files/{fileId}")]
    public async Task<IActionResult> DownloadFile(Guid fileId, CancellationToken cancellationToken)
    {
        try
        {
            var fileData = await _tankService.DownloadTankFileAsync(fileId, cancellationToken);
            if (fileData == null)
            {
                return NotFound(new { message = "File not found" });
            }

            var (fileContent, fileName, contentType) = fileData.Value;

            _logger.LogInformation("File {FileName} downloaded", fileName);

            return File(fileContent, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file {FileId}", fileId);
            return StatusCode(500, new { message = "An error occurred while downloading the file" });
        }
    }

    /// <summary>
    /// Get all files for a tank
    /// </summary>
    [HttpGet("{tankId}/files")]
    public async Task<ActionResult<IEnumerable<TankFileResponse>>> GetTankFiles(Guid tankId, CancellationToken cancellationToken)
    {
        try
        {
            var files = await _tankService.GetTankFilesAsync(tankId, cancellationToken);
            return Ok(files);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving files for tank {TankId}", tankId);
            return StatusCode(500, new { message = "An error occurred while retrieving tank files" });
        }
    }

    /// <summary>
    /// Delete a tank file
    /// </summary>
    [HttpDelete("files/{fileId}")]
    public async Task<ActionResult> DeleteFile(Guid fileId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _tankService.DeleteTankFileAsync(fileId, cancellationToken);
            if (!result)
            {
                return NotFound(new { message = "File not found" });
            }

            return Ok(new { message = "File deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FileId}", fileId);
            return StatusCode(500, new { message = "An error occurred while deleting the file" });
        }
    }
}
