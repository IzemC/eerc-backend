using System.Text;
using System.Text.Json;
using ENOC.Application.Configuration;
using ENOC.Application.DTOs.Notification;
using ENOC.Application.Interfaces;
using ENOC.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ENOC.Infrastructure.Services;

public class PushNotificationService : IPushNotificationService
{
    private readonly FcmConfig _fcmConfig;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDeviceTokenService _deviceTokenService;
    private readonly ILogger<PushNotificationService> _logger;
    private readonly HttpClient _httpClient;
    private const string FcmUrl = "https://fcm.googleapis.com/fcm/send";

    public PushNotificationService(
        IOptions<FcmConfig> fcmConfig,
        IUnitOfWork unitOfWork,
        IDeviceTokenService deviceTokenService,
        ILogger<PushNotificationService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _fcmConfig = fcmConfig.Value;
        _unitOfWork = unitOfWork;
        _deviceTokenService = deviceTokenService;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"key={_fcmConfig.ServerKey}");
        _httpClient.DefaultRequestHeaders.Add("Sender", $"id={_fcmConfig.SenderId}");
    }

    public async Task<bool> SendPushNotificationAsync(PushNotificationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                to = request.DeviceToken,
                notification = new
                {
                    title = request.Title,
                    body = request.Body,
                    sound = request.Sound ?? "default",
                    image = request.ImageUrl
                },
                data = request.Data
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(FcmUrl, content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Push notification sent successfully to device token: {DeviceToken}", MaskToken(request.DeviceToken));
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to send push notification. Status: {StatusCode}, Response: {Response}",
                    response.StatusCode, errorContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending push notification");
            return false;
        }
    }

    public async Task<bool> SendPushNotificationToUserAsync(PushNotificationToUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Repository<ApplicationUser>().GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found", request.UserId);
            return false;
        }

        // Retrieve user's active device tokens
        var deviceTokens = await _deviceTokenService.GetActiveDeviceTokensForUserAsync(request.UserId, cancellationToken);
        if (!deviceTokens.Any())
        {
            _logger.LogWarning("No active device tokens found for user {UserId}", request.UserId);
            return false;
        }

        // Send push notification to all user's devices
        var tasks = deviceTokens.Select(token =>
        {
            var pushRequest = new PushNotificationRequest
            {
                DeviceToken = token,
                Title = request.Title,
                Body = request.Body,
                Data = request.Data,
                ImageUrl = request.ImageUrl
            };
            return SendPushNotificationAsync(pushRequest, cancellationToken);
        });

        var results = await Task.WhenAll(tasks);
        return results.Any(r => r);
    }

    public async Task<bool> SendPushNotificationToTeamAsync(Guid teamId, string title, string body, Dictionary<string, string>? data = null, CancellationToken cancellationToken = default)
    {
        // Retrieve all active device tokens for team members
        var deviceTokens = await _deviceTokenService.GetActiveDeviceTokensForTeamAsync(teamId, cancellationToken);
        if (!deviceTokens.Any())
        {
            _logger.LogWarning("No active device tokens found for team {TeamId}", teamId);
            return false;
        }

        // Send push notification to all team members' devices
        var tasks = deviceTokens.Select(token =>
        {
            var pushRequest = new PushNotificationRequest
            {
                DeviceToken = token,
                Title = title,
                Body = body,
                Data = data ?? new Dictionary<string, string>()
            };
            return SendPushNotificationAsync(pushRequest, cancellationToken);
        });

        var results = await Task.WhenAll(tasks);
        return results.Any(r => r);
    }

    private static string MaskToken(string token)
    {
        if (string.IsNullOrEmpty(token) || token.Length < 10)
            return "***";

        return $"{token[..4]}...{token[^4..]}";
    }
}

/// <summary>
/// Mock push notification service for local development when FcmService feature flag is disabled
/// </summary>
public class MockPushNotificationService : IPushNotificationService
{
    private readonly ILogger<MockPushNotificationService> _logger;

    public MockPushNotificationService(ILogger<MockPushNotificationService> logger)
    {
        _logger = logger;
    }

    public Task<bool> SendPushNotificationAsync(PushNotificationRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[MOCK FCM] Would send push notification - Title: {Title}, Body: {Body}", request.Title, request.Body);
        return Task.FromResult(true);
    }

    public Task<bool> SendPushNotificationToUserAsync(PushNotificationToUserRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[MOCK FCM] Would send push notification to user {UserId} - Title: {Title}", request.UserId, request.Title);
        return Task.FromResult(true);
    }

    public Task<bool> SendPushNotificationToTeamAsync(Guid teamId, string title, string body, Dictionary<string, string>? data = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[MOCK FCM] Would send push notification to team {TeamId} - Title: {Title}", teamId, title);
        return Task.FromResult(true);
    }
}
