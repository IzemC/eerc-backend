using ENOC.Dashboard.Models;

namespace ENOC.Dashboard.Services;

public class AuthStateService
{
    private readonly AuthApiService _authApiService;
    private readonly ILogger<AuthStateService> _logger;

    private string? _accessToken;
    private string? _refreshToken;
    private UserInfo? _currentUser;

    public event Action? OnAuthStateChanged;

    public AuthStateService(AuthApiService authApiService, ILogger<AuthStateService> logger)
    {
        _authApiService = authApiService;
        _logger = logger;
    }

    public bool IsAuthenticated => !string.IsNullOrEmpty(_accessToken) && _currentUser != null;

    public UserInfo? CurrentUser => _currentUser;

    public string? AccessToken => _accessToken;

    public async Task<bool> LoginAsync(string username, string password)
    {
        try
        {
            var response = await _authApiService.LoginAsync(username, password);

            if (response != null && !string.IsNullOrEmpty(response.AccessToken))
            {
                _accessToken = response.AccessToken;
                _refreshToken = response.RefreshToken;
                _currentUser = response.User;

                NotifyAuthStateChanged();
                _logger.LogInformation("User {Username} logged in successfully", username);
                return true;
            }

            _logger.LogWarning("Login failed for user {Username}", username);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user {Username}", username);
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            if (!string.IsNullOrEmpty(_accessToken))
            {
                await _authApiService.LogoutAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
        }
        finally
        {
            _accessToken = null;
            _refreshToken = null;
            _currentUser = null;
            NotifyAuthStateChanged();
        }
    }

    public async Task<bool> RefreshTokenAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(_refreshToken))
            {
                _logger.LogWarning("No refresh token available");
                return false;
            }

            var response = await _authApiService.RefreshTokenAsync(_refreshToken);

            if (response != null && !string.IsNullOrEmpty(response.AccessToken))
            {
                _accessToken = response.AccessToken;
                _refreshToken = response.RefreshToken;
                _currentUser = response.User;

                NotifyAuthStateChanged();
                _logger.LogInformation("Token refreshed successfully");
                return true;
            }

            _logger.LogWarning("Token refresh failed");
            await LogoutAsync();
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            await LogoutAsync();
            return false;
        }
    }

    public async Task<bool> InitializeAsync()
    {
        // In a real app, you might load tokens from secure storage
        // For now, we'll just check if we have a valid token
        if (!string.IsNullOrEmpty(_accessToken))
        {
            try
            {
                var user = await _authApiService.GetCurrentUserAsync();
                if (user != null)
                {
                    _currentUser = user;
                    NotifyAuthStateChanged();
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing auth state");
            }
        }

        return false;
    }

    private void NotifyAuthStateChanged()
    {
        OnAuthStateChanged?.Invoke();
    }
}
