using ENOC.Application.DTOs.Weather;
using ENOC.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ENOC.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WeatherController : ControllerBase
{
    private readonly IWeatherService _weatherService;
    private readonly ILogger<WeatherController> _logger;

    public WeatherController(IWeatherService weatherService, ILogger<WeatherController> logger)
    {
        _weatherService = weatherService;
        _logger = logger;
    }

    /// <summary>
    /// Get current weather data
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<WeatherResponse>> GetCurrentWeather(CancellationToken cancellationToken)
    {
        try
        {
            var weather = await _weatherService.GetCurrentWeatherAsync(cancellationToken);
            return Ok(weather);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving weather data");
            return StatusCode(500, new { message = "An error occurred while retrieving weather data" });
        }
    }
}
