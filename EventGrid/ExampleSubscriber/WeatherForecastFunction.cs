using System;
using System.Text.Json;
using Azure.Messaging.EventGrid;
using Contracts;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace ExampleSubscriber;

public class WeatherForecastFunction
{
    [FunctionName("WeatherForecastFunction")]
    public void Run([ServiceBusTrigger("azure-messaging-services-example-queue", Connection = "ServiceBus")]byte[] message, ILogger log)
    {
        var eventGridMessage = JsonSerializer.Deserialize<EventGridEvent>(message);

        var weatherForecast = JsonSerializer.Deserialize<WeatherForecast[]>(eventGridMessage.Data);

        foreach (var forecast in weatherForecast ?? Array.Empty<WeatherForecast>())
        {
            log.LogInformation($"Weather is {forecast.Summary} and {forecast.TemperatureF}F at {forecast.Date}.");
        }
    }
}