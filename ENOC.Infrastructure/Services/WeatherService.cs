using ENOC.Application.Configuration;
using ENOC.Application.DTOs.Weather;
using ENOC.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace ENOC.Infrastructure.Services;

public class WeatherService : IWeatherService
{
    private readonly WeatherConfig _weatherConfig;
    private readonly ILogger<WeatherService> _logger;
    private readonly HttpClient _httpClient;

    public WeatherService(
        IOptions<WeatherConfig> weatherConfig,
        ILogger<WeatherService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _weatherConfig = weatherConfig.Value;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task<WeatherResponse> GetCurrentWeatherAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(_weatherConfig.ApiUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var weatherData = JsonSerializer.Deserialize<WeatherResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (weatherData == null)
            {
                throw new InvalidOperationException("Failed to deserialize weather data");
            }

            weatherData.Timestamp = DateTime.UtcNow;

            _logger.LogInformation("Retrieved weather data: Temp {Temp}°C, Humidity {Humidity}%",
                weatherData.Temp1, weatherData.RelHumidity);

            return weatherData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving weather data from {Url}", _weatherConfig.ApiUrl);
            throw;
        }
    }
}

/// <summary>
/// Mock weather service for local development when weather station is not accessible
/// </summary>
public class MockWeatherService : IWeatherService
{
    private readonly ILogger<MockWeatherService> _logger;
    private readonly Random _random = new();

    public MockWeatherService(ILogger<MockWeatherService> logger)
    {
        _logger = logger;
    }

    public Task<WeatherResponse> GetCurrentWeatherAsync(CancellationToken cancellationToken = default)
    {
        // Generate realistic mock weather data for Dubai/ENOC location
        var temp = 25m + (decimal)(_random.NextDouble() * 20); // 25-45°C
        var humidity = 30 + _random.Next(50); // 30-80%
        var windSpeed = (decimal)(_random.NextDouble() * 20); // 0-20 m/s

        var mockData = new WeatherResponse
        {
            Temp1 = temp,
            RelHumidity = humidity,
            HeatIndex = temp + (humidity > 50 ? 2m : 0m),
            DewPoint = temp - ((100 - humidity) / 5m),
            WindChill = temp - (windSpeed > 5 ? 1m : 0m),
            RawWindDir = _random.Next(360),
            WindSpeed = windSpeed,
            ThreeSecRollAvgWindSpeed = windSpeed + (decimal)(_random.NextDouble() * 2 - 1),
            ThreeSecRollAvgWindDir = _random.Next(360),
            TwoMinRollAvgWindSpeed = windSpeed + (decimal)(_random.NextDouble() * 1),
            TwoMinRollAvgWindDir = _random.Next(360),
            TenMinRollAvgWindSpeed = windSpeed,
            TenMinRollAvgWindDir = _random.Next(360),
            TenMinWindGustSpeed = windSpeed + (decimal)(_random.NextDouble() * 5),
            TenMinWindGustDir = _random.Next(360),
            SixtyMinWindGustSpeed = windSpeed + (decimal)(_random.NextDouble() * 8),
            SixtyMinWindGustDir = _random.Next(360),
            AdjBaromPress = 1013m + (decimal)(_random.NextDouble() * 20 - 10), // 1003-1023 hPa
            RainToday = (decimal)(_random.NextDouble() * 5), // 0-5mm
            Timestamp = DateTime.UtcNow
        };

        _logger.LogInformation("[MOCK WEATHER] Temp: {Temp}°C, Humidity: {Humidity}%, Wind: {Wind} m/s",
            mockData.Temp1, mockData.RelHumidity, mockData.WindSpeed);

        return Task.FromResult(mockData);
    }
}
