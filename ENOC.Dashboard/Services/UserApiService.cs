using ENOC.Dashboard.Models;

namespace ENOC.Dashboard.Services;

public class UserApiService
{
    private readonly ApiService _apiService;
    private readonly ILogger<UserApiService> _logger;

    public UserApiService(ApiService apiService, ILogger<UserApiService> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    public async Task<List<User>?> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _apiService.GetAsync<List<User>>("/api/users", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            return null;
        }
    }

    public async Task<User?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _apiService.GetAsync<User>($"/api/users/{id}", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {UserId}", id);
            return null;
        }
    }

    public async Task<User?> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _apiService.PostAsync<CreateUserRequest, User>("/api/users", request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return null;
        }
    }

    public async Task<User?> UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _apiService.PutAsync<UpdateUserRequest, User>($"/api/users/{id}", request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return null;
        }
    }

    public async Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _apiService.DeleteAsync($"/api/users/{id}", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            return false;
        }
    }
}
