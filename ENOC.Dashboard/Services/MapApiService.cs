using ENOC.Dashboard.Models;

namespace ENOC.Dashboard.Services;

public class MapApiService
{
    private readonly ApiService _apiService;
    private readonly ILogger<MapApiService> _logger;

    public MapApiService(ApiService apiService, ILogger<MapApiService> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    public async Task<MapData?> GetMapDataAsync(Guid? businessUnitId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = "/api/maps/data";
            if (businessUnitId.HasValue)
                endpoint += $"?businessUnitId={businessUnitId.Value}";

            return await _apiService.GetAsync<MapData>(endpoint, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting map data");
            return null;
        }
    }
}
