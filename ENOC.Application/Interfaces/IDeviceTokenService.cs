using ENOC.Application.DTOs.DeviceToken;

namespace ENOC.Application.Interfaces;

public interface IDeviceTokenService
{
    Task<DeviceTokenResponse> RegisterDeviceTokenAsync(Guid userId, RegisterDeviceTokenRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<DeviceTokenResponse>> GetUserDeviceTokensAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetActiveDeviceTokensForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetActiveDeviceTokensForTeamAsync(Guid teamId, CancellationToken cancellationToken = default);
    Task<bool> DeactivateDeviceTokenAsync(Guid userId, string deviceToken, CancellationToken cancellationToken = default);
    Task<bool> DeleteDeviceTokenAsync(Guid tokenId, CancellationToken cancellationToken = default);
}
