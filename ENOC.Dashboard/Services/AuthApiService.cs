using ENOC.Dashboard.Models;

namespace ENOC.Dashboard.Services;

public class AuthApiService
{
    private readonly ApiService _apiService;
    private readonly ILogger<AuthApiService> _logger;

    public AuthApiService(ApiService apiService, ILogger<AuthApiService> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    public async Task<LoginResponse?> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new { Username = username, Password = password };
            var response = await _apiService.PostAsync<object, LoginResponse>("/api/auth/login", request, cancellationToken);

            if (response != null && !string.IsNullOrEmpty(response.AccessToken))
            {
                // Set the authorization header for future requests
                _apiService.SetAuthorizationHeader(response.AccessToken);
                _logger.LogInformation("User {Username} logged in successfully", username);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user {Username}", username);
            return null;
        }
    }

    public async Task<bool> LogoutAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _apiService.PostAsync("/api/auth/logout", new { }, cancellationToken);
            _apiService.ClearAuthorizationHeader();
            _logger.LogInformation("User logged out");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return false;
        }
    }

    public async Task<UserInfo?> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _apiService.GetAsync<UserInfo>("/api/auth/me", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return null;
        }
    }

    public async Task<LoginResponse?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new { RefreshToken = refreshToken };
            var response = await _apiService.PostAsync<object, LoginResponse>("/api/auth/refresh", request, cancellationToken);

            if (response != null && !string.IsNullOrEmpty(response.AccessToken))
            {
                _apiService.SetAuthorizationHeader(response.AccessToken);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return null;
        }
    }
}
