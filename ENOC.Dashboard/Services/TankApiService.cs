using ENOC.Dashboard.Models;

namespace ENOC.Dashboard.Services;

public class TankApiService
{
    private readonly ApiService _apiService;
    private readonly ILogger<TankApiService> _logger;

    public TankApiService(ApiService apiService, ILogger<TankApiService> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    public async Task<List<Tank>?> GetAllTanksAsync(Guid? businessUnitId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = "/api/tanks";
            if (businessUnitId.HasValue)
                endpoint += $"?businessUnitId={businessUnitId.Value}";

            return await _apiService.GetAsync<List<Tank>>(endpoint, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tanks");
            return null;
        }
    }

    public async Task<Tank?> GetTankByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _apiService.GetAsync<Tank>($"/api/tanks/{id}", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tank {TankId}", id);
            return null;
        }
    }
}
