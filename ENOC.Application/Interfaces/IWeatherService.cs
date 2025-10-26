using ENOC.Application.DTOs.Weather;

namespace ENOC.Application.Interfaces;

public interface IWeatherService
{
    Task<WeatherResponse> GetCurrentWeatherAsync(CancellationToken cancellationToken = default);
}
