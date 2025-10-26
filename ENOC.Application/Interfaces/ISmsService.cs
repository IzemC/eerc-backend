using ENOC.Application.DTOs.Notification;

namespace ENOC.Application.Interfaces;

public interface ISmsService
{
    Task<bool> SendSmsAsync(SmsNotificationRequest request, CancellationToken cancellationToken = default);
    Task<bool> SendSmsToUserAsync(Guid userId, string message, CancellationToken cancellationToken = default);
    Task<bool> SendSmsToTeamAsync(Guid teamId, string message, CancellationToken cancellationToken = default);
}
