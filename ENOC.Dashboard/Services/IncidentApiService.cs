using ENOC.Dashboard.Models;

namespace ENOC.Dashboard.Services;

public class IncidentApiService
{
    private readonly ApiService _apiService;
    private readonly ILogger<IncidentApiService> _logger;

    public IncidentApiService(ApiService apiService, ILogger<IncidentApiService> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    public async Task<List<Incident>?> GetAllIncidentsAsync(string? status = null, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(status)) queryParams.Add($"status={status}");
            if (fromDate.HasValue) queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
            if (toDate.HasValue) queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");

            var endpoint = "/api/incidents";
            if (queryParams.Any())
                endpoint += "?" + string.Join("&", queryParams);

            return await _apiService.GetAsync<List<Incident>>(endpoint, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting incidents");
            return null;
        }
    }

    public async Task<Incident?> GetIncidentByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _apiService.GetAsync<Incident>($"/api/incidents/{id}", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting incident {IncidentId}", id);
            return null;
        }
    }

    public async Task<Incident?> CreateIncidentAsync(CreateIncidentRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _apiService.PostAsync<CreateIncidentRequest, Incident>("/api/incidents", request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating incident");
            return null;
        }
    }

    public async Task<bool> CloseIncidentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _apiService.PostAsync($"/api/incidents/{id}/close", new { }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing incident {IncidentId}", id);
            return false;
        }
    }

    public async Task<bool> AcknowledgeIncidentAsync(Guid id, AcknowledgeIncidentRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _apiService.PostAsync($"/api/incidents/{id}/acknowledge", request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acknowledging incident {IncidentId}", id);
            return false;
        }
    }

    public async Task<List<IncidentAcknowledgement>?> GetIncidentAcknowledgementsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _apiService.GetAsync<List<IncidentAcknowledgement>>($"/api/incidents/{id}/acknowledgements", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting acknowledgements for incident {IncidentId}", id);
            return null;
        }
    }
}
