using System.Text;
using System.Text.Json;
using ENOC.Application.Configuration;
using ENOC.Application.DTOs.Notification;
using ENOC.Application.Interfaces;
using ENOC.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ENOC.Infrastructure.Services;

public class SmsService : ISmsService
{
    private readonly SmsConfig _smsConfig;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SmsService> _logger;
    private readonly HttpClient _httpClient;

    public SmsService(IOptions<SmsConfig> smsConfig, IUnitOfWork unitOfWork, ILogger<SmsService> logger, IHttpClientFactory httpClientFactory)
    {
        _smsConfig = smsConfig.Value;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task<bool> SendSmsAsync(SmsNotificationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                sender_id = _smsConfig.SenderId,
                phone_number = request.PhoneNumber,
                message = request.Message,
                api_key = _smsConfig.ApiKey
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_smsConfig.ApiUrl, content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("SMS sent successfully to {PhoneNumber}", request.PhoneNumber);
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to send SMS to {PhoneNumber}. Status: {StatusCode}, Response: {Response}",
                    request.PhoneNumber, response.StatusCode, errorContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS to {PhoneNumber}", request.PhoneNumber);
            return false;
        }
    }

    public async Task<bool> SendSmsToUserAsync(Guid userId, string message, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Repository<ApplicationUser>().GetByIdAsync(userId, cancellationToken);
        if (user == null || string.IsNullOrEmpty(user.PhoneNumber))
        {
            _logger.LogWarning("User {UserId} not found or has no phone number", userId);
            return false;
        }

        var request = new SmsNotificationRequest
        {
            PhoneNumber = user.PhoneNumber,
            Message = message
        };

        return await SendSmsAsync(request, cancellationToken);
    }

    public async Task<bool> SendSmsToTeamAsync(Guid teamId, string message, CancellationToken cancellationToken = default)
    {
        var users = (await _unitOfWork.Repository<ApplicationUser>().GetAllAsync(cancellationToken))
            .Where(u => u.TeamId == teamId && !string.IsNullOrEmpty(u.PhoneNumber))
            .ToList();

        if (!users.Any())
        {
            _logger.LogWarning("No users found in team {TeamId} with phone numbers", teamId);
            return false;
        }

        var tasks = users.Select(user => SendSmsToUserAsync(user.Id, message, cancellationToken));
        var results = await Task.WhenAll(tasks);

        return results.All(r => r);
    }
}

/// <summary>
/// Mock SMS service for local development when SmsService feature flag is disabled
/// </summary>
public class MockSmsService : ISmsService
{
    private readonly ILogger<MockSmsService> _logger;

    public MockSmsService(ILogger<MockSmsService> logger)
    {
        _logger = logger;
    }

    public Task<bool> SendSmsAsync(SmsNotificationRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[MOCK SMS] Would send SMS to {PhoneNumber}: {Message}", request.PhoneNumber, request.Message);
        return Task.FromResult(true);
    }

    public Task<bool> SendSmsToUserAsync(Guid userId, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[MOCK SMS] Would send SMS to user {UserId}: {Message}", userId, message);
        return Task.FromResult(true);
    }

    public Task<bool> SendSmsToTeamAsync(Guid teamId, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[MOCK SMS] Would send SMS to team {TeamId}: {Message}", teamId, message);
        return Task.FromResult(true);
    }
}
