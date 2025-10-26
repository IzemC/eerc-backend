using ENOC.Dashboard.Models;

namespace ENOC.Dashboard.Services;

public class LookupApiService
{
    private readonly ApiService _apiService;
    private readonly ILogger<LookupApiService> _logger;

    public LookupApiService(ApiService apiService, ILogger<LookupApiService> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    public async Task<List<BusinessUnit>?> GetBusinessUnitsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _apiService.GetAsync<List<BusinessUnit>>("/api/lookup/business-units", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting business units");
            return null;
        }
    }

    public async Task<List<IncidentType>?> GetIncidentTypesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _apiService.GetAsync<List<IncidentType>>("/api/lookup/incident-types", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting incident types");
            return null;
        }
    }

    public async Task<List<Message>?> GetMessagesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _apiService.GetAsync<List<Message>>("/api/lookup/messages", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting messages");
            return null;
        }
    }

    public async Task<List<Tank>?> GetTanksAsync(Guid? businessUnitId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = "/api/lookup/tanks";
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

    public async Task<List<Team>?> GetTeamsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _apiService.GetAsync<List<Team>>("/api/lookup/teams", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting teams");
            return null;
        }
    }

    public async Task<List<Vehicle>?> GetVehiclesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _apiService.GetAsync<List<Vehicle>>("/api/lookup/vehicles", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vehicles");
            return null;
        }
    }

    public async Task<List<EercPosition>?> GetEercPositionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _apiService.GetAsync<List<EercPosition>>("/api/lookup/eerc-positions", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting EERC positions");
            return null;
        }
    }
}
