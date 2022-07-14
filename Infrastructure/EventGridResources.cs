using System.Collections.Generic;
using Pulumi;
using AzureNative = Pulumi.AzureNative;
using AzureAD = Pulumi.AzureAD;

namespace Infrastructure;

public class EventGridResources
{
    public EventGridResources(string prefix,
        AzureNative.Resources.ResourceGroup resourceGroup,
        AzureAD.GetUserResult jimmyUser)
    {
        var exampleTopic = new AzureNative.EventGrid.Topic($"{prefix}-example-topic", new AzureNative.EventGrid.TopicArgs
        {
            ResourceGroupName = resourceGroup.Name
        });

        var eventGridJimmySenderRoleAssignment = new AzureNative.Authorization.RoleAssignment(
            $"{prefix}-example-topic-event-grid-data-sender-jimmy",
            new AzureNative.Authorization.RoleAssignmentArgs
            {
                PrincipalId = jimmyUser.ObjectId,
                PrincipalType = "User",
                Scope = exampleTopic.Id,
                // This is "EventGrid Data Sender" role
                RoleDefinitionId =
                    "/providers/Microsoft.Authorization/roleDefinitions/d5a91429-5739-47e2-a06b-3470a27159e7"
            });

        var exampleSubscriberNamespace = new AzureNative.ServiceBus.Namespace($"{prefix}-sb-namespace",
            new AzureNative.ServiceBus.NamespaceArgs
            {
                ResourceGroupName = resourceGroup.Name,
                Sku = new AzureNative.ServiceBus.Inputs.SBSkuArgs
                {
                    Name = AzureNative.ServiceBus.SkuName.Basic,
                    Tier = AzureNative.ServiceBus.SkuTier.Basic
                }
            });
        var exampleSubscriberQueue = new AzureNative.ServiceBus.Queue($"{prefix}-example-queue",
            new AzureNative.ServiceBus.QueueArgs
            {
                NamespaceName = exampleSubscriberNamespace.Name,
                ResourceGroupName = resourceGroup.Name
            });

        var queueJimmyDataReceiverRoleAssignment = new AzureNative.Authorization.RoleAssignment(
            $"{prefix}-example-queue-data-receiver-jimmy",
            new AzureNative.Authorization.RoleAssignmentArgs
            {
                PrincipalId = jimmyUser.ObjectId,
                PrincipalType = "User",
                Scope = exampleSubscriberQueue.Id,
                // This is "Azure Service Bus Data Receiver" role
                RoleDefinitionId = "/providers/Microsoft.Authorization/roleDefinitions/4f6d3b9b-027b-4f4c-9142-0e5a2a2247e0"
            });

        var queueJimmyDataOwnerRoleAssignment = new AzureNative.Authorization.RoleAssignment(
            $"{prefix}-example-queue-data-owner-jimmy",
            new AzureNative.Authorization.RoleAssignmentArgs
            {
                PrincipalId = jimmyUser.ObjectId,
                PrincipalType = "User",
                Scope = exampleSubscriberQueue.Id,
                // This is "Azure Service Bus Data Owner" role
                RoleDefinitionId = "/providers/Microsoft.Authorization/roleDefinitions/090c5cfd-751d-490a-894a-3ce6f1109419"
            });


        var eventGridQueueSubscription = new AzureNative.EventGrid.TopicEventSubscription(
            $"{prefix}-example-topic-queue-sub",
            new AzureNative.EventGrid.TopicEventSubscriptionArgs
            {
                ResourceGroupName = resourceGroup.Name,
                TopicName = exampleTopic.Name,
                Destination = new AzureNative.EventGrid.Inputs.ServiceBusQueueEventSubscriptionDestinationArgs
                {
                    ResourceId = exampleSubscriberQueue.Id,
                    EndpointType = AzureNative.EventGrid.EndpointType.ServiceBusQueue.ToString()
                },
                Filter = new AzureNative.EventGrid.Inputs.EventSubscriptionFilterArgs
                {
                    IncludedEventTypes =
                    {
                    "ExamplePublisher.WeatherForecast"
                    }
                }
            }
        );

        ExampleTopicEndpoint = exampleTopic.Endpoint;
        ExampleTopicId = exampleTopic.Id;
        ServiceBusName = exampleSubscriberNamespace.Name;
        ServiceBusEndpoint = exampleSubscriberNamespace.ServiceBusEndpoint;
        ServiceBusQueue = exampleSubscriberQueue.Name;
    }

    public Output<string> ServiceBusQueue { get; set; }

    public Output<string> ServiceBusEndpoint { get; set; }

    public Output<string> ServiceBusName { get; set; }

    public Output<string> ExampleTopicId { get; set; }

    public Output<string> ExampleTopicEndpoint { get; set; }
}