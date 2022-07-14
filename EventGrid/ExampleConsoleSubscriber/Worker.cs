using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Messaging.EventGrid;
using Azure.Messaging.ServiceBus;

namespace ExampleConsoleSubscriber;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ServiceBusClient _client;

    public Worker(ILogger<Worker> logger, ServiceBusClient client)
    {
        _logger = logger;
        _client = client;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var receiver = _client.CreateReceiver("azure-messaging-services-example-queue");
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            var message = await receiver.ReceiveMessageAsync(null, stoppingToken);

            if (message != null)
            {
                var eventGridMessage = JsonSerializer.Deserialize<EventGridEvent>(message.Body);
                var contents = Encoding.UTF8.GetString(eventGridMessage.Data);

                _logger.LogInformation(contents);
            }

            await Task.Delay(1000, stoppingToken);
        }
    }
}