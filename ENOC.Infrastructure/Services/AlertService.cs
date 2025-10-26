using ENOC.Application.DTOs.Alert;
using ENOC.Application.Interfaces;
using ENOC.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ENOC.Infrastructure.Services;

public class AlertService : IAlertService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly IPushNotificationService _pushNotificationService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<AlertService> _logger;

    public AlertService(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        ISmsService smsService,
        IPushNotificationService pushNotificationService,
        INotificationService notificationService,
        ILogger<AlertService> logger)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _smsService = smsService;
        _pushNotificationService = pushNotificationService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<bool> SendAlertToTeamsAsync(SendAlertRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!request.TeamIds.Any())
            {
                _logger.LogWarning("No teams specified for alert");
                return false;
            }

            var teams = (await _unitOfWork.Repository<Team>().GetAllAsync(cancellationToken))
                .Where(t => request.TeamIds.Contains(t.Id))
                .ToList();

            if (!teams.Any())
            {
                _logger.LogWarning("No valid teams found for alert");
                return false;
            }

            var teamNames = string.Join(", ", teams.Select(t => t.Name));
            _logger.LogInformation("Sending alert to teams: {Teams}", teamNames);

            var results = new List<bool>();

            // Send to each team
            foreach (var team in teams)
            {
                // Send push notification
                if (request.SendPushNotification)
                {
                    try
                    {
                        var pushResult = await _pushNotificationService.SendPushNotificationToTeamAsync(
                            team.Id,
                            "EERC Alert",
                            request.Message,
                            new Dictionary<string, string>
                            {
                                { "type", "alert" },
                                { "team", team.Name }
                            });
                        results.Add(pushResult);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending push notification to team {TeamId}", team.Id);
                    }
                }

                // Send email
                if (request.SendEmail)
                {
                    try
                    {
                        var emailResult = await _emailService.SendEmailToTeamAsync(
                            team.Id,
                            "EERC Alert",
                            request.Message,
                            false, // isHtml
                            cancellationToken);
                        results.Add(emailResult);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending email to team {TeamId}", team.Id);
                    }
                }

                // Send SMS
                if (request.SendSms)
                {
                    try
                    {
                        var smsResult = await _smsService.SendSmsToTeamAsync(
                            team.Id,
                            request.Message,
                            cancellationToken);
                        results.Add(smsResult);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending SMS to team {TeamId}", team.Id);
                    }
                }

                // Send SignalR notification to team group
                try
                {
                    await _notificationService.NotifyTeamAsync(team.Id, request.Message, "alert", cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending SignalR notification to team {TeamId}", team.Id);
                }
            }

            _logger.LogInformation("Alert sent to {TeamCount} teams", teams.Count);

            // Return true if at least one notification was successful
            return results.Any(r => r);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending alert to teams");
            throw;
        }
    }
}
