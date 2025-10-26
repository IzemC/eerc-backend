namespace ENOC.Application.Interfaces;

/// <summary>
/// Interface for SignalR notification hub client methods
/// These methods are called on the client side
/// </summary>
public interface INotificationHub
{
    Task ReceiveIncidentCreated(object incident);
    Task ReceiveIncidentUpdated(object incident);
    Task ReceiveIncidentClosed(object incident);
    Task ReceiveIncidentAcknowledged(object acknowledgement);
    Task ReceiveNotification(string message, string type);
}
