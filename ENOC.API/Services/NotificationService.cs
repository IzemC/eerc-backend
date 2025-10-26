using ENOC.API.Hubs;
using ENOC.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace ENOC.API.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub, INotificationHub> _hubContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IHubContext<NotificationHub, INotificationHub> hubContext,
        ILogger<NotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyIncidentCreatedAsync(Guid incidentId, object incident, CancellationToken cancellationToken = default)
    {
        try
        {
            // Notify all users
            await _hubContext.Clients.Group("all_users").ReceiveIncidentCreated(incident);

            // Notify incident-specific group
            await _hubContext.Clients.Group($"incident_{incidentId}").ReceiveIncidentCreated(incident);

            _logger.LogInformation("Notified users about incident creation: {IncidentId}", incidentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying incident creation: {IncidentId}", incidentId);
        }
    }

    public async Task NotifyIncidentUpdatedAsync(Guid incidentId, object incident, CancellationToken cancellationToken = default)
    {
        try
        {
            // Notify all users
            await _hubContext.Clients.Group("all_users").ReceiveIncidentUpdated(incident);

            // Notify incident-specific group
            await _hubContext.Clients.Group($"incident_{incidentId}").ReceiveIncidentUpdated(incident);

            _logger.LogInformation("Notified users about incident update: {IncidentId}", incidentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying incident update: {IncidentId}", incidentId);
        }
    }

    public async Task NotifyIncidentClosedAsync(Guid incidentId, object incident, CancellationToken cancellationToken = default)
    {
        try
        {
            // Notify all users
            await _hubContext.Clients.Group("all_users").ReceiveIncidentClosed(incident);

            // Notify incident-specific group
            await _hubContext.Clients.Group($"incident_{incidentId}").ReceiveIncidentClosed(incident);

            _logger.LogInformation("Notified users about incident closure: {IncidentId}", incidentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying incident closure: {IncidentId}", incidentId);
        }
    }

    public async Task NotifyIncidentAcknowledgedAsync(Guid incidentId, object acknowledgement, CancellationToken cancellationToken = default)
    {
        try
        {
            // Notify all users
            await _hubContext.Clients.Group("all_users").ReceiveIncidentAcknowledged(acknowledgement);

            // Notify incident-specific group
            await _hubContext.Clients.Group($"incident_{incidentId}").ReceiveIncidentAcknowledged(acknowledgement);

            _logger.LogInformation("Notified users about incident acknowledgement: {IncidentId}", incidentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying incident acknowledgement: {IncidentId}", incidentId);
        }
    }

    public async Task NotifyUserAsync(Guid userId, string message, string type, CancellationToken cancellationToken = default)
    {
        try
        {
            await _hubContext.Clients.Group($"user_{userId}").ReceiveNotification(message, type);
            _logger.LogInformation("Notified user {UserId}: {Message}", userId, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying user {UserId}", userId);
        }
    }

    public async Task NotifyTeamAsync(Guid teamId, string message, string type, CancellationToken cancellationToken = default)
    {
        try
        {
            await _hubContext.Clients.Group($"team_{teamId}").ReceiveNotification(message, type);
            _logger.LogInformation("Notified team {TeamId}: {Message}", teamId, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying team {TeamId}", teamId);
        }
    }

    public async Task NotifyAllAsync(string message, string type, CancellationToken cancellationToken = default)
    {
        try
        {
            await _hubContext.Clients.Group("all_users").ReceiveNotification(message, type);
            _logger.LogInformation("Notified all users: {Message}", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying all users");
        }
    }
}
