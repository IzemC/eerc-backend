namespace ENOC.Application.Interfaces;

public interface INotificationService
{
    Task NotifyIncidentCreatedAsync(Guid incidentId, object incident, CancellationToken cancellationToken = default);
    Task NotifyIncidentUpdatedAsync(Guid incidentId, object incident, CancellationToken cancellationToken = default);
    Task NotifyIncidentClosedAsync(Guid incidentId, object incident, CancellationToken cancellationToken = default);
    Task NotifyIncidentAcknowledgedAsync(Guid incidentId, object acknowledgement, CancellationToken cancellationToken = default);
    Task NotifyUserAsync(Guid userId, string message, string type, CancellationToken cancellationToken = default);
    Task NotifyTeamAsync(Guid teamId, string message, string type, CancellationToken cancellationToken = default);
    Task NotifyAllAsync(string message, string type, CancellationToken cancellationToken = default);
}
