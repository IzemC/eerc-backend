using ENOC.Application.DTOs.Notification;

namespace ENOC.Application.Interfaces;

public interface IEmailService
{
    Task<bool> SendEmailAsync(EmailNotificationRequest request, CancellationToken cancellationToken = default);
    Task<bool> SendEmailToUserAsync(Guid userId, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default);
    Task<bool> SendEmailToTeamAsync(Guid teamId, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default);
}
