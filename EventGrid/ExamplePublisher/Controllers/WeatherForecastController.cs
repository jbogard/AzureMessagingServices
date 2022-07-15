using Azure;
using Azure.Messaging.EventGrid;
using Contracts;
using Microsoft.AspNetCore.Mvc;

namespace ExamplePublisher.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries =
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly EventGridPublisherClient _client;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, EventGridPublisherClient client)
    {
        _logger = logger;
        _client = client;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<IEnumerable<WeatherForecast>> Get()
    {
        _logger.LogInformation("Publishing sample event grid event");
        var payload = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();

        var eventGridEvents = payload.Select(e => new EventGridEvent(
            "WeatherForecast",
            "ExamplePublisher.WeatherForecast",
            "1.0",
            e));

        try
        {
            await _client.SendEventsAsync(eventGridEvents);

        }
        catch (RequestFailedException e)
        {
            _logger.LogError(e, "Request failed.");
            throw;
        }
        return payload;
    }
}