using ENOC.Application.DTOs.DeviceToken;
using ENOC.Application.Interfaces;
using ENOC.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ENOC.Infrastructure.Services;

public class DeviceTokenService : IDeviceTokenService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeviceTokenService> _logger;

    public DeviceTokenService(IUnitOfWork unitOfWork, ILogger<DeviceTokenService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<DeviceTokenResponse> RegisterDeviceTokenAsync(Guid userId, RegisterDeviceTokenRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var tokenRepo = _unitOfWork.Repository<UserDeviceToken>();

            // Check if this device token already exists for this user
            var existingToken = (await tokenRepo.GetAllAsync(cancellationToken))
                .FirstOrDefault(t => t.UserId == userId && t.DeviceToken == request.DeviceToken);

            if (existingToken != null)
            {
                // Update existing token
                existingToken.DeviceType = request.DeviceType;
                existingToken.DeviceName = request.DeviceName;
                existingToken.IsActive = true;
                existingToken.LastUsedAt = DateTime.UtcNow;

                tokenRepo.Update(existingToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Updated device token for user {UserId}", userId);

                return MapToResponse(existingToken);
            }

            // Create new token
            var deviceToken = new UserDeviceToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                DeviceToken = request.DeviceToken,
                DeviceType = request.DeviceType,
                DeviceName = request.DeviceName,
                IsActive = true,
                RegisteredAt = DateTime.UtcNow,
                LastUsedAt = DateTime.UtcNow
            };

            await tokenRepo.AddAsync(deviceToken, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Registered new device token for user {UserId}", userId);

            return MapToResponse(deviceToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering device token for user {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<DeviceTokenResponse>> GetUserDeviceTokensAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tokens = (await _unitOfWork.Repository<UserDeviceToken>().GetAllAsync(cancellationToken))
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.LastUsedAt ?? t.RegisteredAt)
            .ToList();

        return tokens.Select(MapToResponse);
    }

    public async Task<IEnumerable<string>> GetActiveDeviceTokensForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tokens = (await _unitOfWork.Repository<UserDeviceToken>().GetAllAsync(cancellationToken))
            .Where(t => t.UserId == userId && t.IsActive)
            .Select(t => t.DeviceToken)
            .ToList();

        return tokens;
    }

    public async Task<IEnumerable<string>> GetActiveDeviceTokensForTeamAsync(Guid teamId, CancellationToken cancellationToken = default)
    {
        // Get all users in the team
        var users = (await _unitOfWork.Repository<ApplicationUser>().GetAllAsync(cancellationToken))
            .Where(u => u.TeamId == teamId && u.IsActive && !u.IsDeleted)
            .Select(u => u.Id)
            .ToList();

        // Get all active device tokens for these users
        var tokens = (await _unitOfWork.Repository<UserDeviceToken>().GetAllAsync(cancellationToken))
            .Where(t => users.Contains(t.UserId) && t.IsActive)
            .Select(t => t.DeviceToken)
            .Distinct()
            .ToList();

        return tokens;
    }

    public async Task<bool> DeactivateDeviceTokenAsync(Guid userId, string deviceToken, CancellationToken cancellationToken = default)
    {
        var tokenRepo = _unitOfWork.Repository<UserDeviceToken>();

        var token = (await tokenRepo.GetAllAsync(cancellationToken))
            .FirstOrDefault(t => t.UserId == userId && t.DeviceToken == deviceToken);

        if (token == null)
        {
            return false;
        }

        token.IsActive = false;
        tokenRepo.Update(token);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deactivated device token for user {UserId}", userId);

        return true;
    }

    public async Task<bool> DeleteDeviceTokenAsync(Guid tokenId, CancellationToken cancellationToken = default)
    {
        var token = await _unitOfWork.Repository<UserDeviceToken>().GetByIdAsync(tokenId, cancellationToken);
        if (token == null)
        {
            return false;
        }

        _unitOfWork.Repository<UserDeviceToken>().Remove(token);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted device token {TokenId} for user {UserId}", tokenId, token.UserId);

        return true;
    }

    private static DeviceTokenResponse MapToResponse(UserDeviceToken token)
    {
        return new DeviceTokenResponse
        {
            Id = token.Id,
            UserId = token.UserId,
            DeviceToken = token.DeviceToken,
            DeviceType = token.DeviceType,
            DeviceName = token.DeviceName,
            IsActive = token.IsActive,
            RegisteredAt = token.RegisteredAt,
            LastUsedAt = token.LastUsedAt
        };
    }
}
