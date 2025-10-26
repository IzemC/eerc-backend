using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ENOC.Dashboard.Configuration;
using Microsoft.Extensions.Options;

namespace ENOC.Dashboard.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly ApiConfig _apiConfig;
    private readonly ILogger<ApiService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiService(
        HttpClient httpClient,
        IOptions<ApiConfig> apiConfig,
        ILogger<ApiService> logger)
    {
        _httpClient = httpClient;
        _apiConfig = apiConfig.Value;
        _logger = logger;

        // Configure JSON options
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Set base address and timeout
        _httpClient.BaseAddress = new Uri(_apiConfig.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_apiConfig.Timeout);
    }

    public void SetAuthorizationHeader(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public void ClearAuthorizationHeader()
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    public async Task<T?> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("GET request to {Endpoint}", endpoint);
            var response = await _httpClient.GetAsync(endpoint, cancellationToken);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<T>(_jsonOptions, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error calling GET {Endpoint}", endpoint);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling GET {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("POST request to {Endpoint}", endpoint);
            var response = await _httpClient.PostAsJsonAsync(endpoint, data, _jsonOptions, cancellationToken);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error calling POST {Endpoint}", endpoint);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling POST {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<bool> PostAsync<TRequest>(string endpoint, TRequest data, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("POST request to {Endpoint}", endpoint);
            var response = await _httpClient.PostAsJsonAsync(endpoint, data, _jsonOptions, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error calling POST {Endpoint}", endpoint);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling POST {Endpoint}", endpoint);
            return false;
        }
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest data, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("PUT request to {Endpoint}", endpoint);
            var response = await _httpClient.PutAsJsonAsync(endpoint, data, _jsonOptions, cancellationToken);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error calling PUT {Endpoint}", endpoint);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling PUT {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("DELETE request to {Endpoint}", endpoint);
            var response = await _httpClient.DeleteAsync(endpoint, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error calling DELETE {Endpoint}", endpoint);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling DELETE {Endpoint}", endpoint);
            return false;
        }
    }
}
