using ENOC.Application.DTOs.Notification;

namespace ENOC.Application.Interfaces;

public interface IPushNotificationService
{
    Task<bool> SendPushNotificationAsync(PushNotificationRequest request, CancellationToken cancellationToken = default);
    Task<bool> SendPushNotificationToUserAsync(PushNotificationToUserRequest request, CancellationToken cancellationToken = default);
    Task<bool> SendPushNotificationToTeamAsync(Guid teamId, string title, string body, Dictionary<string, string>? data = null, CancellationToken cancellationToken = default);
}
