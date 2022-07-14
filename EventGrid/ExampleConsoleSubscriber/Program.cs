using Azure.Core;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using ExampleConsoleSubscriber;
using Microsoft.Extensions.Azure;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((host, services) =>
    {
        var options = new DefaultAzureCredentialOptions
        {
            VisualStudioTenantId = ""
        };
        services.AddSingleton<TokenCredential>(new DefaultAzureCredential(options));
        services.AddTransient(s =>
            new ServiceBusClient("azure-messaging-services-sb-namespacec603049c.servicebus.windows.net",
                s.GetRequiredService<TokenCredential>(), new ServiceBusClientOptions
                {
                    TransportType = ServiceBusTransportType.AmqpWebSockets
                }));
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
