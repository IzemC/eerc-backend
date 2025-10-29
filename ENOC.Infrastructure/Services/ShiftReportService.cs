using ENOC.Application.DTOs.ShiftReport;
using ENOC.Application.Interfaces;
using ENOC.Domain.Entities;
using ENOC.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ENOC.Infrastructure.Services;

public class ShiftReportService : IShiftReportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ShiftReportService> _logger;

    public ShiftReportService(IUnitOfWork unitOfWork, ApplicationDbContext context, ILogger<ShiftReportService> logger)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _logger = logger;
    }

    public async Task<ShiftReportResponse?> CreateShiftReportAsync(CreateShiftReportRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Generate shift form counter and ID
            var shiftReportRepo = _unitOfWork.Repository<ShiftReportForm>();
            var lastReport = (await shiftReportRepo.GetAllAsync(cancellationToken))
                .OrderByDescending(s => s.ShiftFormCounter)
                .FirstOrDefault();

            var counter = (lastReport?.ShiftFormCounter ?? 0) + 1;
            var shiftFormId = $"SHIFT-{DateTime.UtcNow:yyyyMMdd}-{counter:D4}";

            var shiftReport = new ShiftReportForm
            {
                Id = Guid.NewGuid(),
                ShiftFormId = shiftFormId,
                ShiftFormCounter = counter,
                UserId = userId,
                TeamId = request.TeamId,
                From = request.From,
                To = request.To,
                Name = request.Name,
                Activities = null,
                CreatedAt = DateTime.UtcNow
            };

            await shiftReportRepo.AddAsync(shiftReport, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Shift report {ShiftFormId} created by user {UserId}", shiftFormId, userId);

            return await GetShiftReportByIdAsync(shiftReport.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating shift report");
            throw;
        }
    }

    public async Task<ShiftReportResponse?> GetShiftReportByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var report = await _unitOfWork.Repository<ShiftReportForm>().GetByIdAsync(id, cancellationToken);
        if (report == null)
        {
            return null;
        }

        var user = await _unitOfWork.Repository<ApplicationUser>().GetByIdAsync(report.UserId, cancellationToken);
        var team = await _unitOfWork.Repository<Team>().GetByIdAsync(report.TeamId, cancellationToken);

        // Get vehicle statuses with eager loading of Vehicle
        var vehicleStatusEntries = await _context.ShiftReportVehicleStatuses
            .Include(vs => vs.Vehicle)
            .Where(vs => vs.ShiftReportFormId == report.Id)
            .ToListAsync(cancellationToken);

        var vehicleStatuses = vehicleStatusEntries.Select(status => new ShiftReportVehicleStatusResponse
        {
            Id = status.Id,
            ShiftReportFormId = status.ShiftReportFormId,
            VehicleId = status.VehicleId,
            VehicleName = status.Vehicle?.Name ?? "Unknown",
            Description = status.Description,
            CreatedAt = status.CreatedAt
        }).ToList();

        return new ShiftReportResponse
        {
            Id = report.Id,
            ShiftFormId = report.ShiftFormId,
            ShiftFormCounter = report.ShiftFormCounter,
            UserId = report.UserId,
            UserName = user?.UserName ?? "Unknown",
            UserEmployeeId = user?.EmployeeId ?? "Unknown",
            TeamId = report.TeamId,
            TeamName = team?.Name ?? "Unknown",
            From = report.From,
            To = report.To,
            Name = report.Name,
            Activities = report.Activities,
            CreatedAt = report.CreatedAt,
            VehicleStatuses = vehicleStatuses
        };
    }

    public async Task<IEnumerable<ShiftReportResponse>> GetAllShiftReportsAsync(Guid? teamId = null, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var reports = await _unitOfWork.Repository<ShiftReportForm>().GetAllAsync(cancellationToken);

        if (teamId.HasValue)
        {
            reports = reports.Where(r => r.TeamId == teamId.Value);
        }

        if (fromDate.HasValue)
        {
            reports = reports.Where(r => r.From >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            reports = reports.Where(r => r.To <= toDate.Value);
        }

        var responses = new List<ShiftReportResponse>();
        foreach (var report in reports.OrderByDescending(r => r.CreatedAt))
        {
            var response = await GetShiftReportByIdAsync(report.Id, cancellationToken);
            if (response != null)
            {
                responses.Add(response);
            }
        }

        return responses;
    }

    public async Task<bool> DeleteShiftReportAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var report = await _unitOfWork.Repository<ShiftReportForm>().GetByIdAsync(id, cancellationToken);
        if (report == null)
        {
            return false;
        }

        _unitOfWork.Repository<ShiftReportForm>().Remove(report);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Shift report {ShiftFormId} deleted", report.ShiftFormId);

        return true;
    }

    public async Task<ShiftReportVehicleStatusResponse?> AddVehicleStatusAsync(Guid shiftReportId, Guid vehicleId, string? description, CancellationToken cancellationToken = default)
    {
        try
        {
            var shiftReport = await _unitOfWork.Repository<ShiftReportForm>().GetByIdAsync(shiftReportId, cancellationToken);
            if (shiftReport == null)
            {
                _logger.LogWarning("Shift report {ShiftReportId} not found", shiftReportId);
                return null;
            }

            var vehicle = await _unitOfWork.Repository<Vehicle>().GetByIdAsync(vehicleId, cancellationToken);
            if (vehicle == null)
            {
                _logger.LogWarning("Vehicle {VehicleId} not found", vehicleId);
                return null;
            }

            var vehicleStatus = new ShiftReportVehicleStatus
            {
                Id = Guid.NewGuid(),
                ShiftReportFormId = shiftReportId,
                VehicleId = vehicleId,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<ShiftReportVehicleStatus>().AddAsync(vehicleStatus, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Vehicle status added for vehicle {VehicleId} in shift report {ShiftReportId}", vehicleId, shiftReportId);

            return new ShiftReportVehicleStatusResponse
            {
                Id = vehicleStatus.Id,
                ShiftReportFormId = vehicleStatus.ShiftReportFormId,
                VehicleId = vehicleStatus.VehicleId,
                VehicleName = vehicle.Name,
                Description = vehicleStatus.Description,
                CreatedAt = vehicleStatus.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding vehicle status");
            throw;
        }
    }

    public async Task<List<ShiftReportVehicleStatusResponse>> AddBulkVehicleStatusAsync(Guid shiftReportId, AddBulkVehicleStatusRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var shiftReport = await _unitOfWork.Repository<ShiftReportForm>().GetByIdAsync(shiftReportId, cancellationToken);
            if (shiftReport == null)
            {
                _logger.LogWarning("Shift report {ShiftReportId} not found", shiftReportId);
                return new List<ShiftReportVehicleStatusResponse>();
            }

            var responses = new List<ShiftReportVehicleStatusResponse>();
            var vehicleStatuses = new List<ShiftReportVehicleStatus>();

            foreach (var item in request.VehicleStatuses)
            {
                var vehicle = await _unitOfWork.Repository<Vehicle>().GetByIdAsync(item.VehicleId, cancellationToken);
                if (vehicle == null)
                {
                    _logger.LogWarning("Vehicle {VehicleId} not found, skipping", item.VehicleId);
                    continue;
                }

                var vehicleStatus = new ShiftReportVehicleStatus
                {
                    Id = Guid.NewGuid(),
                    ShiftReportFormId = shiftReportId,
                    VehicleId = item.VehicleId,
                    Description = item.Description,
                    CreatedAt = DateTime.UtcNow
                };

                vehicleStatuses.Add(vehicleStatus);

                responses.Add(new ShiftReportVehicleStatusResponse
                {
                    Id = vehicleStatus.Id,
                    ShiftReportFormId = vehicleStatus.ShiftReportFormId,
                    VehicleId = vehicleStatus.VehicleId,
                    VehicleName = vehicle.Name,
                    Description = vehicleStatus.Description,
                    CreatedAt = vehicleStatus.CreatedAt
                });
            }

            if (vehicleStatuses.Any())
            {
                await _unitOfWork.Repository<ShiftReportVehicleStatus>().AddRangeAsync(vehicleStatuses, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Added {Count} vehicle statuses to shift report {ShiftReportId}", vehicleStatuses.Count, shiftReportId);
            }

            return responses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding bulk vehicle statuses");
            throw;
        }
    }

    public async Task<IEnumerable<ShiftReportVehicleStatusResponse>> GetVehicleStatusesByShiftReportIdAsync(Guid shiftReportId, CancellationToken cancellationToken = default)
    {
        var statuses = await _context.ShiftReportVehicleStatuses
            .Include(vs => vs.Vehicle)
            .Where(vs => vs.ShiftReportFormId == shiftReportId)
            .ToListAsync(cancellationToken);

        return statuses.Select(status => new ShiftReportVehicleStatusResponse
        {
            Id = status.Id,
            ShiftReportFormId = status.ShiftReportFormId,
            VehicleId = status.VehicleId,
            VehicleName = status.Vehicle?.Name ?? "Unknown",
            Description = status.Description,
            CreatedAt = status.CreatedAt
        }).ToList();
    }

    public async Task<bool> DeleteVehicleStatusAsync(Guid vehicleStatusId, CancellationToken cancellationToken = default)
    {
        var status = await _unitOfWork.Repository<ShiftReportVehicleStatus>().GetByIdAsync(vehicleStatusId, cancellationToken);
        if (status == null)
        {
            return false;
        }

        _unitOfWork.Repository<ShiftReportVehicleStatus>().Remove(status);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Vehicle status {VehicleStatusId} deleted", vehicleStatusId);

        return true;
    }

    public async Task<ShiftReportResponse?> UpdateActivitiesAsync(Guid shiftReportId, string activities, CancellationToken cancellationToken = default)
    {
        var report = await _unitOfWork.Repository<ShiftReportForm>().GetByIdAsync(shiftReportId, cancellationToken);
        if (report == null)
        {
            return null;
        }

        report.Activities = activities;
        _unitOfWork.Repository<ShiftReportForm>().Update(report);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Activities updated for shift report {ShiftFormId}", report.ShiftFormId);

        return await GetShiftReportByIdAsync(shiftReportId, cancellationToken);
    }
}
