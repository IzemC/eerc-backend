using ENOC.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ENOC.API.Hubs;

[Authorize]
public class NotificationHub : Hub<INotificationHub>
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            // Add user to their personal group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");

            // Get user's team and add to team group
            var teamId = Context.User?.FindFirst("TeamId")?.Value;
            if (!string.IsNullOrEmpty(teamId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"team_{teamId}");
                _logger.LogInformation("User {Username} (ID: {UserId}) connected to team {TeamId}", username, userId, teamId);
            }
            else
            {
                _logger.LogInformation("User {Username} (ID: {UserId}) connected", username, userId);
            }

            // Add to global notifications group
            await Groups.AddToGroupAsync(Context.ConnectionId, "all_users");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value;

        if (exception != null)
        {
            _logger.LogWarning(exception, "User {Username} (ID: {UserId}) disconnected with error", username, userId);
        }
        else
        {
            _logger.LogInformation("User {Username} (ID: {UserId}) disconnected", username, userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Subscribe to incident-specific updates
    /// </summary>
    public async Task SubscribeToIncident(string incidentId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"incident_{incidentId}");
        _logger.LogInformation("Connection {ConnectionId} subscribed to incident {IncidentId}", Context.ConnectionId, incidentId);
    }

    /// <summary>
    /// Unsubscribe from incident-specific updates
    /// </summary>
    public async Task UnsubscribeFromIncident(string incidentId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"incident_{incidentId}");
        _logger.LogInformation("Connection {ConnectionId} unsubscribed from incident {IncidentId}", Context.ConnectionId, incidentId);
    }

    /// <summary>
    /// Send a message to all users
    /// </summary>
    public async Task BroadcastMessage(string message)
    {
        var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
        await Clients.All.ReceiveNotification($"{username}: {message}", "info");
    }
}
