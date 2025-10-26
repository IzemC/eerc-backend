using ENOC.Application.Interfaces;

namespace ENOC.Infrastructure.Services;

/// <summary>
/// Stub implementation - actual implementation is in ENOC.API.Services
/// This is here to prevent circular dependency between Infrastructure and API
/// </summary>
public class NotificationService : INotificationService
{
    public Task NotifyIncidentCreatedAsync(Guid incidentId, object incident, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task NotifyIncidentUpdatedAsync(Guid incidentId, object incident, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task NotifyIncidentClosedAsync(Guid incidentId, object incident, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task NotifyIncidentAcknowledgedAsync(Guid incidentId, object acknowledgement, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task NotifyUserAsync(Guid userId, string message, string type, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task NotifyTeamAsync(Guid teamId, string message, string type, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task NotifyAllAsync(string message, string type, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
