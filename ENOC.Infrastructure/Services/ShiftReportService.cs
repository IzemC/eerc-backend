using ENOC.Application.DTOs.ShiftReport;
using ENOC.Application.Interfaces;
using ENOC.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ENOC.Infrastructure.Services;

public class ShiftReportService : IShiftReportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ShiftReportService> _logger;

    public ShiftReportService(IUnitOfWork unitOfWork, ILogger<ShiftReportService> logger)
    {
        _unitOfWork = unitOfWork;
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
                IncidentId = request.IncidentId,
                Name = request.Name,
                Details = request.Details,
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
        var incident = report.IncidentId.HasValue ? await _unitOfWork.Repository<Incident>().GetByIdAsync(report.IncidentId.Value, cancellationToken) : null;

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
            IncidentId = report.IncidentId,
            IncidentNumber = incident?.IncidentId,
            Name = report.Name,
            Details = report.Details,
            CreatedAt = report.CreatedAt
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
}
