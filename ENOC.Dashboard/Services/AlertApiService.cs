using ENOC.Dashboard.Models;

namespace ENOC.Dashboard.Services;

public class AlertApiService
{
    private readonly ApiService _apiService;
    private readonly ILogger<AlertApiService> _logger;

    public AlertApiService(ApiService apiService, ILogger<AlertApiService> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    public async Task<bool> SendAlertAsync(SendAlertRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _apiService.PostAsync("/api/alerts/send", request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending alert");
            return false;
        }
    }
}
