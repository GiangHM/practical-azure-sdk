using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Abstractions;

namespace VictorNetCoreApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class OnBehalfController : ControllerBase
    {
        private readonly IDownstreamApi _downstreamApi;
        public OnBehalfController( IDownstreamApi downstreamApi)
        {
            _downstreamApi = downstreamApi;
        }
        [HttpGet("GetWeatherForecastObo")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetWeatherForecastOBOAsync()
        {
            // Simulate fetching weather data on behalf of the user
            using var weatherResponse = await _downstreamApi.CallApiForAppAsync(
                "DownstreamApi",
                options =>
                {
                    options.HttpMethod = "Get";
                    options.RelativePath = "WeatherForecast/GetWeatherForecast";
                });
            if (!weatherResponse.IsSuccessStatusCode)
            {
                return StatusCode((int)weatherResponse.StatusCode, "Failed to fetch weather data.");
            }
            var weatherData = await weatherResponse.Content.ReadAsStringAsync();

            // Return the weather data
            return Ok(weatherData);
        }
    }
}
