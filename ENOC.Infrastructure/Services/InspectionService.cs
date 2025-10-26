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
            CreatedAt = inspection.CreatedAt
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
}
