using System.Text.Json;
using Azure.Core;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Contracts;
using Microsoft.AspNetCore.Mvc;

namespace ExamplePublisher.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly EventHubProducerClient _producer;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, EventHubProducerClient producer)
    {
        _logger = logger;
        _producer = producer;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<IEnumerable<WeatherForecast>> Get()
    {
        var weatherForecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();

        _logger.LogInformation("Publishing events to Event Hub...");

        using var eventBatch = await _producer.CreateBatchAsync();

        foreach (var forecast in weatherForecasts)
        {
            eventBatch.TryAdd(new EventData(BinaryData.FromObjectAsJson(forecast)));
        }

        await _producer.SendAsync(eventBatch);

        return weatherForecasts;
    }
}