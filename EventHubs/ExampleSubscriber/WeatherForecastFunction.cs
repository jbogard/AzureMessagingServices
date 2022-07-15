using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Contracts;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace ExampleSubscriber
{
    public static class WeatherForecastFunction
    {
        [FunctionName("WeatherForecastFunction")]
        public static async Task Run([EventHubTrigger("azure-messaging-services-weather-forecast", Connection = "EventHubs")] EventData[] events, ILogger log)
        {
            var exceptions = new List<Exception>();

            foreach (var eventData in events)
            {
                try
                {
                    var forecast = JsonSerializer.Deserialize<WeatherForecast>(eventData.EventBody)!;

                    log.LogInformation($"Weather is {forecast.Summary} and {forecast.TemperatureF}F at {forecast.Date}.");

                    await Task.Yield();
                }
                catch (Exception e)
                {
                    // We need to keep processing the rest of the batch - capture this exception and continue.
                    // Also, consider capturing details of the message that failed processing so it can be processed again later.
                    exceptions.Add(e);
                }
            }

            // Once processing of the batch is complete, if any messages in the batch failed processing throw an exception so that there is a record of the failure.

            if (exceptions.Count > 1)
                throw new AggregateException(exceptions);

            if (exceptions.Count == 1)
                throw exceptions.Single();
        }
    }
}
