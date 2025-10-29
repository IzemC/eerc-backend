using ENOC.Application.DTOs.Incident;
using ENOC.Application.Interfaces;
using ENOC.Domain.Entities;
using ENOC.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ENOC.Infrastructure.Services;

public class IncidentService : IIncidentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<IncidentService> _logger;

    public IncidentService(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        ILogger<IncidentService> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<IncidentResponse?> CreateIncidentAsync(CreateIncidentRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Generate incident counter and ID
            var incidentRepo = _unitOfWork.Repository<Incident>();
            var lastIncident = (await incidentRepo.GetAllAsync(cancellationToken))
                .OrderByDescending(i => i.IncidentCounter)
                .FirstOrDefault();

            var counter = (lastIncident?.IncidentCounter ?? 0) + 1;
            var incidentId = $"INC-{DateTime.UtcNow:yyyyMMdd}-{counter:D4}";

            var incident = new Incident
            {
                Id = Guid.NewGuid(),
                IncidentId = incidentId,
                IncidentCounter = counter,
                CreatedAt = DateTime.UtcNow,
                IncidentTypeId = request.IncidentTypeId,
                UnitId = request.UnitId,
                UserId = userId,
                MessageId = request.MessageId,
                TankId = request.TankId,
                Status = IncidentStatus.OPEN,
                ReporterName = request.ReporterName,
                ReporterContactDetails = request.ReporterContactDetails,
                Team = request.Team,
                CustomMessage = request.CustomMessage,
                Action = request.Action
            };

            await incidentRepo.AddAsync(incident, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Incident {IncidentId} created by user {UserId}", incidentId, userId);

            var response = await GetIncidentByIdAsync(incident.Id, cancellationToken);

            // Send real-time notification
            if (response != null)
            {
                await _notificationService.NotifyIncidentCreatedAsync(incident.Id, response, cancellationToken);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating incident");
            throw;
        }
    }

    public async Task<IncidentResponse?> GetIncidentByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var incident = await _unitOfWork.Repository<Incident>().GetByIdAsync(id, cancellationToken);
        if (incident == null)
        {
            return null;
        }

        // Load related entities
        var incidentType = await _unitOfWork.Repository<IncidentType>().GetByIdAsync(incident.IncidentTypeId, cancellationToken);
        var unit = await _unitOfWork.Repository<BusinessUnit>().GetByIdAsync(incident.UnitId, cancellationToken);
        var user = await _unitOfWork.Repository<ApplicationUser>().GetByIdAsync(incident.UserId, cancellationToken);
        var message = await _unitOfWork.Repository<Message>().GetByIdAsync(incident.MessageId, cancellationToken);
        var tank = incident.TankId.HasValue ? await _unitOfWork.Repository<Tank>().GetByIdAsync(incident.TankId.Value, cancellationToken) : null;

        var acknowledgements = await _unitOfWork.Repository<IncidentAcknowledgement>()
            .FindAsync(a => a.IncidentId == id, cancellationToken);

        var ackResponses = new List<IncidentAcknowledgementResponse>();
        foreach (var ack in acknowledgements)
        {
            var ackUser = await _unitOfWork.Repository<ApplicationUser>().GetByIdAsync(ack.UserId, cancellationToken);
            ackResponses.Add(new IncidentAcknowledgementResponse
            {
                Id = ack.Id,
                UserId = ack.UserId,
                UserName = ackUser?.UserName ?? "Unknown",
                UserEmployeeId = ackUser?.EmployeeId ?? "Unknown",
                ETA = ack.ETA,
                AcknowledgementStatus = ack.AcknowledgementStatus,
                CreatedAt = ack.CreatedAt
            });
        }

        return new IncidentResponse
        {
            Id = incident.Id,
            IncidentId = incident.IncidentId,
            IncidentCounter = incident.IncidentCounter,
            CreatedAt = incident.CreatedAt,
            IncidentTypeId = incident.IncidentTypeId,
            IncidentTypeName = incidentType?.Name ?? "Unknown",
            IncidentTypeImage = incidentType?.Image,
            UnitId = incident.UnitId,
            UnitName = unit?.Name ?? "Unknown",
            UserId = incident.UserId,
            UserName = user?.UserName ?? "Unknown",
            UserEmployeeId = user?.EmployeeId ?? "Unknown",
            MessageId = incident.MessageId,
            MessageDescription = message?.Description ?? "Unknown",
            TankId = incident.TankId,
            TankName = tank?.Name,
            TankNumber = tank?.TankNumber,
            Status = incident.Status,
            ClosedAt = incident.ClosedAt,
            ReporterName = incident.ReporterName,
            ReporterContactDetails = incident.ReporterContactDetails,
            Team = incident.Team,
            CustomMessage = incident.CustomMessage,
            Action = incident.Action,
            Description = incident.Description,
            TimeOfTurnout = incident.TimeOfTurnout,
            TimeOfArrival = incident.TimeOfArrival,
            TimeOfAllClear = incident.TimeOfAllClear,
            Acknowledgements = ackResponses
        };
    }

    public async Task<IEnumerable<IncidentResponse>> GetAllIncidentsAsync(IncidentStatus? status = null, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var incidents = await _unitOfWork.Repository<Incident>().GetAllAsync(cancellationToken);

        // Apply filters
        if (status.HasValue)
        {
            incidents = incidents.Where(i => i.Status == status.Value);
        }

        if (fromDate.HasValue)
        {
            incidents = incidents.Where(i => i.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            incidents = incidents.Where(i => i.CreatedAt <= toDate.Value);
        }

        var responses = new List<IncidentResponse>();
        foreach (var incident in incidents.OrderByDescending(i => i.CreatedAt))
        {
            var response = await GetIncidentByIdAsync(incident.Id, cancellationToken);
            if (response != null)
            {
                responses.Add(response);
            }
        }

        return responses;
    }

    public async Task<IncidentResponse?> UpdateIncidentAsync(Guid id, UpdateIncidentRequest request, CancellationToken cancellationToken = default)
    {
        var incident = await _unitOfWork.Repository<Incident>().GetByIdAsync(id, cancellationToken);
        if (incident == null)
        {
            return null;
        }

        // Update only provided fields
        if (request.IncidentTypeId.HasValue)
            incident.IncidentTypeId = request.IncidentTypeId.Value;

        if (request.UnitId.HasValue)
            incident.UnitId = request.UnitId.Value;

        if (request.MessageId.HasValue)
            incident.MessageId = request.MessageId.Value;

        if (request.TankId.HasValue)
            incident.TankId = request.TankId.Value;

        if (request.Status.HasValue)
        {
            incident.Status = request.Status.Value;
            if (request.Status.Value == IncidentStatus.CLOSE && !incident.ClosedAt.HasValue)
            {
                incident.ClosedAt = DateTime.UtcNow;
            }
        }

        if (request.ReporterName != null)
            incident.ReporterName = request.ReporterName;

        if (request.ReporterContactDetails != null)
            incident.ReporterContactDetails = request.ReporterContactDetails;

        if (request.Team != null)
            incident.Team = request.Team;

        if (request.CustomMessage != null)
            incident.CustomMessage = request.CustomMessage;

        if (request.Action != null)
            incident.Action = request.Action;

        if (request.Description != null)
            incident.Description = request.Description;

        if (request.TimeOfTurnout.HasValue)
            incident.TimeOfTurnout = request.TimeOfTurnout;

        if (request.TimeOfArrival.HasValue)
            incident.TimeOfArrival = request.TimeOfArrival;

        if (request.TimeOfAllClear.HasValue)
            incident.TimeOfAllClear = request.TimeOfAllClear;

        _unitOfWork.Repository<Incident>().Update(incident);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Incident {IncidentId} updated", incident.IncidentId);

        var response = await GetIncidentByIdAsync(id, cancellationToken);

        // Send real-time notification
        if (response != null)
        {
            await _notificationService.NotifyIncidentUpdatedAsync(id, response, cancellationToken);
        }

        return response;
    }

    public async Task<bool> CloseIncidentAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var incident = await _unitOfWork.Repository<Incident>().GetByIdAsync(id, cancellationToken);
        if (incident == null)
        {
            return false;
        }

        incident.Status = IncidentStatus.CLOSE;
        incident.ClosedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Incident>().Update(incident);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Incident {IncidentId} closed by user {UserId}", incident.IncidentId, userId);

        // Send real-time notification
        var response = await GetIncidentByIdAsync(id, cancellationToken);
        if (response != null)
        {
            await _notificationService.NotifyIncidentClosedAsync(id, response, cancellationToken);
        }

        return true;
    }

    public async Task<bool> AcknowledgeIncidentAsync(Guid incidentId, Guid userId, AcknowledgeIncidentRequest request, CancellationToken cancellationToken = default)
    {
        // Check if incident exists
        var incident = await _unitOfWork.Repository<Incident>().GetByIdAsync(incidentId, cancellationToken);
        if (incident == null)
        {
            return false;
        }

        // Check if user already acknowledged
        var existingAck = await _unitOfWork.Repository<IncidentAcknowledgement>()
            .FirstOrDefaultAsync(a => a.IncidentId == incidentId && a.UserId == userId, cancellationToken);

        if (existingAck != null)
        {
            // Update existing acknowledgement
            existingAck.ETA = request.ETA;
            existingAck.AcknowledgementStatus = request.AcknowledgementStatus;
            _unitOfWork.Repository<IncidentAcknowledgement>().Update(existingAck);
        }
        else
        {
            // Create new acknowledgement
            var acknowledgement = new IncidentAcknowledgement
            {
                Id = Guid.NewGuid(),
                IncidentId = incidentId,
                UserId = userId,
                ETA = request.ETA,
                AcknowledgementStatus = request.AcknowledgementStatus,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<IncidentAcknowledgement>().AddAsync(acknowledgement, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Incident {IncidentId} acknowledged by user {UserId}", incident.IncidentId, userId);

        // Send real-time notification
        var user = await _unitOfWork.Repository<ApplicationUser>().GetByIdAsync(userId, cancellationToken);
        var ackResponse = new IncidentAcknowledgementResponse
        {
            Id = existingAck?.Id ?? Guid.NewGuid(),
            UserId = userId,
            UserName = user?.UserName ?? "Unknown",
            UserEmployeeId = user?.EmployeeId ?? "Unknown",
            ETA = request.ETA,
            AcknowledgementStatus = request.AcknowledgementStatus,
            CreatedAt = DateTime.UtcNow
        };
        await _notificationService.NotifyIncidentAcknowledgedAsync(incidentId, ackResponse, cancellationToken);

        return true;
    }

    public async Task<IEnumerable<IncidentAcknowledgementResponse>> GetIncidentAcknowledgementsAsync(Guid incidentId, CancellationToken cancellationToken = default)
    {
        var acknowledgements = await _unitOfWork.Repository<IncidentAcknowledgement>()
            .FindAsync(a => a.IncidentId == incidentId, cancellationToken);

        var responses = new List<IncidentAcknowledgementResponse>();
        foreach (var ack in acknowledgements)
        {
            var user = await _unitOfWork.Repository<ApplicationUser>().GetByIdAsync(ack.UserId, cancellationToken);
            responses.Add(new IncidentAcknowledgementResponse
            {
                Id = ack.Id,
                UserId = ack.UserId,
                UserName = user?.UserName ?? "Unknown",
                UserEmployeeId = user?.EmployeeId ?? "Unknown",
                ETA = ack.ETA,
                AcknowledgementStatus = ack.AcknowledgementStatus,
                CreatedAt = ack.CreatedAt
            });
        }

        return responses.OrderBy(r => r.CreatedAt);
    }
}
