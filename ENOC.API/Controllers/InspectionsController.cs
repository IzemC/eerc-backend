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

    /// <summary>
    /// Add a defect to an inspection
    /// </summary>
    [HttpPost("{inspectionId}/defects")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<InspectionDefectResponse>> AddDefect(
        Guid inspectionId,
        string questionId,
        string questionText,
        string description,
        IFormFile? image,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(questionId) || string.IsNullOrWhiteSpace(questionText) || string.IsNullOrWhiteSpace(description))
            {
                return BadRequest(new { message = "Question ID, question text, and description are required" });
            }

            // Process image if provided
            byte[]? imageContent = null;
            string? imageFileName = null;
            string? imageContentType = null;
            long? imageSize = null;

            if (image != null && image.Length > 0)
            {
                // Validate file type
                var allowedContentTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/bmp", "image/webp" };
                if (!allowedContentTypes.Contains(image.ContentType.ToLower()))
                {
                    return BadRequest(new { message = $"File type {image.ContentType} is not supported. Only images are allowed." });
                }

                // Validate file size (max 10MB)
                const long maxFileSize = 10 * 1024 * 1024;
                if (image.Length > maxFileSize)
                {
                    return BadRequest(new { message = "Image size exceeds 10MB limit" });
                }

                // Read image content
                using (var memoryStream = new MemoryStream())
                {
                    await image.CopyToAsync(memoryStream, cancellationToken);
                    imageContent = memoryStream.ToArray();
                }

                imageFileName = image.FileName;
                imageContentType = image.ContentType;
                imageSize = image.Length;
            }

            var defect = await _inspectionService.AddDefectAsync(
                inspectionId,
                questionId,
                questionText,
                description,
                imageContent,
                imageFileName,
                imageContentType,
                imageSize,
                cancellationToken);

            if (defect == null)
            {
                return NotFound(new { message = "Inspection not found" });
            }

            _logger.LogInformation("Defect {DefectId} added to inspection {InspectionId}", defect.Id, inspectionId);

            return CreatedAtAction(nameof(GetDefectsByInspection), new { inspectionId }, defect);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding defect to inspection {InspectionId}", inspectionId);
            return StatusCode(500, new { message = "An error occurred while adding the defect" });
        }
    }

    /// <summary>
    /// Get all defects for an inspection
    /// </summary>
    [HttpGet("{inspectionId}/defects")]
    public async Task<ActionResult<IEnumerable<InspectionDefectResponse>>> GetDefectsByInspection(Guid inspectionId, CancellationToken cancellationToken)
    {
        try
        {
            var defects = await _inspectionService.GetDefectsByInspectionIdAsync(inspectionId, cancellationToken);
            return Ok(defects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving defects for inspection {InspectionId}", inspectionId);
            return StatusCode(500, new { message = "An error occurred while retrieving defects" });
        }
    }

    /// <summary>
    /// Download a defect image
    /// </summary>
    [HttpGet("defects/{defectId}/image")]
    public async Task<IActionResult> DownloadDefectImage(Guid defectId, CancellationToken cancellationToken)
    {
        try
        {
            var imageData = await _inspectionService.GetDefectImageAsync(defectId, cancellationToken);
            if (imageData == null)
            {
                return NotFound(new { message = "Defect image not found" });
            }

            var (content, fileName, contentType) = imageData.Value;

            _logger.LogInformation("Defect image {DefectId} downloaded", defectId);

            return File(content, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading defect image {DefectId}", defectId);
            return StatusCode(500, new { message = "An error occurred while downloading the image" });
        }
    }

    /// <summary>
    /// Delete a defect
    /// </summary>
    [HttpDelete("defects/{defectId}")]
    public async Task<ActionResult> DeleteDefect(Guid defectId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _inspectionService.DeleteDefectAsync(defectId, cancellationToken);
            if (!result)
            {
                return NotFound(new { message = "Defect not found" });
            }

            return Ok(new { message = "Defect deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting defect {DefectId}", defectId);
            return StatusCode(500, new { message = "An error occurred while deleting the defect" });
        }
    }
}
