using System.Net;
using System.Net.Mail;
using ENOC.Application.Configuration;
using ENOC.Application.DTOs.Notification;
using ENOC.Application.Interfaces;
using ENOC.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ENOC.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailConfig _emailConfig;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailConfig> emailConfig, IUnitOfWork unitOfWork, ILogger<EmailService> logger)
    {
        _emailConfig = emailConfig.Value;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(EmailNotificationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailConfig.FromEmail, _emailConfig.FromName),
                Subject = request.Subject,
                Body = request.Body,
                IsBodyHtml = request.IsHtml
            };

            mailMessage.To.Add(request.To);

            foreach (var cc in request.Cc)
            {
                mailMessage.CC.Add(cc);
            }

            foreach (var bcc in request.Bcc)
            {
                mailMessage.Bcc.Add(bcc);
            }

            foreach (var attachment in request.Attachments)
            {
                var stream = new MemoryStream(attachment.Content);
                mailMessage.Attachments.Add(new Attachment(stream, attachment.FileName, attachment.ContentType));
            }

            using var smtpClient = new SmtpClient(_emailConfig.SmtpServer, _emailConfig.SmtpPort)
            {
                EnableSsl = _emailConfig.UseSsl
            };

            if (!string.IsNullOrEmpty(_emailConfig.Username) && !string.IsNullOrEmpty(_emailConfig.Password))
            {
                smtpClient.Credentials = new NetworkCredential(_emailConfig.Username, _emailConfig.Password);
            }

            await smtpClient.SendMailAsync(mailMessage, cancellationToken);

            _logger.LogInformation("Email sent successfully to {To} with subject: {Subject}", request.To, request.Subject);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {To} with subject: {Subject}", request.To, request.Subject);
            return false;
        }
    }

    public async Task<bool> SendEmailToUserAsync(Guid userId, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Repository<ApplicationUser>().GetByIdAsync(userId, cancellationToken);
        if (user == null || string.IsNullOrEmpty(user.Email))
        {
            _logger.LogWarning("User {UserId} not found or has no email address", userId);
            return false;
        }

        var request = new EmailNotificationRequest
        {
            To = user.Email,
            Subject = subject,
            Body = body,
            IsHtml = isHtml
        };

        return await SendEmailAsync(request, cancellationToken);
    }

    public async Task<bool> SendEmailToTeamAsync(Guid teamId, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default)
    {
        var users = (await _unitOfWork.Repository<ApplicationUser>().GetAllAsync(cancellationToken))
            .Where(u => u.TeamId == teamId && !string.IsNullOrEmpty(u.Email))
            .ToList();

        if (!users.Any())
        {
            _logger.LogWarning("No users found in team {TeamId} with email addresses", teamId);
            return false;
        }

        var tasks = users.Select(user => SendEmailToUserAsync(user.Id, subject, body, isHtml, cancellationToken));
        var results = await Task.WhenAll(tasks);

        return results.All(r => r);
    }
}

/// <summary>
/// Mock email service for local development when EmailService feature flag is disabled
/// </summary>
public class MockEmailService : IEmailService
{
    private readonly ILogger<MockEmailService> _logger;

    public MockEmailService(ILogger<MockEmailService> logger)
    {
        _logger = logger;
    }

    public Task<bool> SendEmailAsync(EmailNotificationRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[MOCK EMAIL] Would send email to {To} with subject: {Subject}", request.To, request.Subject);
        _logger.LogDebug("[MOCK EMAIL] Body: {Body}", request.Body);
        return Task.FromResult(true);
    }

    public Task<bool> SendEmailToUserAsync(Guid userId, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[MOCK EMAIL] Would send email to user {UserId} with subject: {Subject}", userId, subject);
        return Task.FromResult(true);
    }

    public Task<bool> SendEmailToTeamAsync(Guid teamId, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[MOCK EMAIL] Would send email to team {TeamId} with subject: {Subject}", teamId, subject);
        return Task.FromResult(true);
    }
}
