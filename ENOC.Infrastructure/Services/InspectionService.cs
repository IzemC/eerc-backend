using ENOC.Application.DTOs.Inspection;
using ENOC.Application.Interfaces;
using ENOC.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ENOC.Infrastructure.Services;

public class InspectionService : IInspectionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<InspectionService> _logger;

    public InspectionService(IUnitOfWork unitOfWork, ILogger<InspectionService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<InspectionResponse?> CreateInspectionAsync(CreateInspectionRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get user to copy their signature
            var user = await _unitOfWork.Repository<ApplicationUser>().GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                _logger.LogError("User {UserId} not found when creating inspection", userId);
                return null;
            }

            // Generate inspection counter and ID
            var inspectionRepo = _unitOfWork.Repository<Inspection>();
            var lastInspection = (await inspectionRepo.GetAllAsync(cancellationToken))
                .OrderByDescending(i => i.InspectionCounter)
                .FirstOrDefault();

            var counter = (lastInspection?.InspectionCounter ?? 0) + 1;
            var inspectionId = $"INSP-{DateTime.UtcNow:yyyyMMdd}-{counter:D4}";

            var inspection = new Inspection
            {
                Id = Guid.NewGuid(),
                InspectionId = inspectionId,
                InspectionCounter = counter,
                UserId = userId,
                VehicleId = request.VehicleId,
                Answers = request.Answers,
                IsDefected = request.IsDefected,
                UserSignature = user.Signature, // Copy user's signature image
                UserSignatureContentType = user.SignatureContentType,
                CreatedAt = DateTime.UtcNow
            };

            await inspectionRepo.AddAsync(inspection, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Inspection {InspectionId} created by user {UserId}", inspectionId, userId);

            return await GetInspectionByIdAsync(inspection.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating inspection");
            throw;
        }
    }

    public async Task<InspectionResponse?> GetInspectionByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var inspection = await _unitOfWork.Repository<Inspection>().GetByIdAsync(id, cancellationToken);
        if (inspection == null)
        {
            return null;
        }

        var user = await _unitOfWork.Repository<ApplicationUser>().GetByIdAsync(inspection.UserId, cancellationToken);
        var vehicle = await _unitOfWork.Repository<Vehicle>().GetByIdAsync(inspection.VehicleId, cancellationToken);

        // Get defects for this inspection
        var defects = await GetDefectsByInspectionIdAsync(id, cancellationToken);

        return new InspectionResponse
        {
            Id = inspection.Id,
            InspectionId = inspection.InspectionId,
            InspectionCounter = inspection.InspectionCounter,
            UserId = inspection.UserId,
            UserName = user?.UserName ?? "Unknown",
            UserEmployeeId = user?.EmployeeId ?? "Unknown",
            VehicleId = inspection.VehicleId,
            VehicleName = vehicle?.Name ?? "Unknown",
            VehiclePlateNumber = vehicle?.PlateNumber,
            Answers = inspection.Answers,
            IsDefected = inspection.IsDefected,
            UserSignature = inspection.UserSignature,
            UserSignatureContentType = inspection.UserSignatureContentType,
            CreatedAt = inspection.CreatedAt,
            Defects = defects.ToList()
        };
    }

    public async Task<IEnumerable<InspectionResponse>> GetAllInspectionsAsync(Guid? vehicleId = null, bool? isDefected = null, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var inspections = await _unitOfWork.Repository<Inspection>().GetAllAsync(cancellationToken);

        if (vehicleId.HasValue)
        {
            inspections = inspections.Where(i => i.VehicleId == vehicleId.Value);
        }

        if (isDefected.HasValue)
        {
            inspections = inspections.Where(i => i.IsDefected == isDefected.Value);
        }

        if (fromDate.HasValue)
        {
            inspections = inspections.Where(i => i.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            inspections = inspections.Where(i => i.CreatedAt <= toDate.Value);
        }

        var responses = new List<InspectionResponse>();
        foreach (var inspection in inspections.OrderByDescending(i => i.CreatedAt))
        {
            var response = await GetInspectionByIdAsync(inspection.Id, cancellationToken);
            if (response != null)
            {
                responses.Add(response);
            }
        }

        return responses;
    }

    public async Task<bool> DeleteInspectionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var inspection = await _unitOfWork.Repository<Inspection>().GetByIdAsync(id, cancellationToken);
        if (inspection == null)
        {
            return false;
        }

        _unitOfWork.Repository<Inspection>().Remove(inspection);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Inspection {InspectionId} deleted", inspection.InspectionId);

        return true;
    }

    public async Task<InspectionDefectResponse?> AddDefectAsync(Guid inspectionId, string questionId, string questionText, string description, byte[]? image, string? imageFileName, string? imageContentType, long? imageSize, CancellationToken cancellationToken = default)
    {
        try
        {
            // Verify inspection exists
            var inspection = await _unitOfWork.Repository<Inspection>().GetByIdAsync(inspectionId, cancellationToken);
            if (inspection == null)
            {
                return null;
            }

            var defect = new InspectionDefect
            {
                Id = Guid.NewGuid(),
                InspectionId = inspectionId,
                QuestionId = questionId,
                QuestionText = questionText,
                Description = description,
                Image = image,
                ImageFileName = imageFileName,
                ImageContentType = imageContentType,
                ImageSize = imageSize,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<InspectionDefect>().AddAsync(defect, cancellationToken);

            // Update inspection IsDefected flag
            inspection.IsDefected = true;
            _unitOfWork.Repository<Inspection>().Update(inspection);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Defect {DefectId} added to inspection {InspectionId}", defect.Id, inspectionId);

            return new InspectionDefectResponse
            {
                Id = defect.Id,
                InspectionId = defect.InspectionId,
                QuestionId = defect.QuestionId,
                QuestionText = defect.QuestionText,
                Description = defect.Description,
                HasImage = defect.Image != null,
                ImageFileName = defect.ImageFileName,
                ImageSize = defect.ImageSize,
                CreatedAt = defect.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding defect to inspection {InspectionId}", inspectionId);
            throw;
        }
    }

    public async Task<IEnumerable<InspectionDefectResponse>> GetDefectsByInspectionIdAsync(Guid inspectionId, CancellationToken cancellationToken = default)
    {
        var defects = (await _unitOfWork.Repository<InspectionDefect>().GetAllAsync(cancellationToken))
            .Where(d => d.InspectionId == inspectionId)
            .OrderBy(d => d.CreatedAt);

        return defects.Select(d => new InspectionDefectResponse
        {
            Id = d.Id,
            InspectionId = d.InspectionId,
            QuestionId = d.QuestionId,
            QuestionText = d.QuestionText,
            Description = d.Description,
            HasImage = d.Image != null,
            ImageFileName = d.ImageFileName,
            ImageSize = d.ImageSize,
            CreatedAt = d.CreatedAt
        });
    }

    public async Task<(byte[] content, string fileName, string contentType)?> GetDefectImageAsync(Guid defectId, CancellationToken cancellationToken = default)
    {
        var defect = await _unitOfWork.Repository<InspectionDefect>().GetByIdAsync(defectId, cancellationToken);
        if (defect == null || defect.Image == null)
        {
            return null;
        }

        return (defect.Image, defect.ImageFileName ?? "defect-image", defect.ImageContentType ?? "application/octet-stream");
    }

    public async Task<bool> DeleteDefectAsync(Guid defectId, CancellationToken cancellationToken = default)
    {
        var defect = await _unitOfWork.Repository<InspectionDefect>().GetByIdAsync(defectId, cancellationToken);
        if (defect == null)
        {
            return false;
        }

        _unitOfWork.Repository<InspectionDefect>().Remove(defect);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Defect {DefectId} deleted", defectId);

        return true;
    }
}
