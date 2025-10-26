using ENOC.Application.DTOs.Auth;

namespace ENOC.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<bool> LogoutAsync(Guid userId, string? application = null, CancellationToken cancellationToken = default);
    Task<bool> ValidateAdCredentialsAsync(string username, string password);
}
