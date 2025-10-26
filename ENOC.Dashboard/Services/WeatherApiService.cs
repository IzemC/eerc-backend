using ENOC.Dashboard.Models;

namespace ENOC.Dashboard.Services;

public class WeatherApiService
{
    private readonly ApiService _apiService;
    private readonly ILogger<WeatherApiService> _logger;

    public WeatherApiService(ApiService apiService, ILogger<WeatherApiService> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    public async Task<Weather?> GetCurrentWeatherAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _apiService.GetAsync<Weather>("/api/weather", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current weather");
            return null;
        }
    }
}
