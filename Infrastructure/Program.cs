using System.Collections.Generic;
using Pulumi;
using AzureNative = Pulumi.AzureNative;
using AzureAD = Pulumi.AzureAD;
using Infrastructure;

await Deployment.RunAsync(async () =>
{
    // Create an Azure Resource Group
    var prefix = "azure-messaging-services";

    var resourceGroup = new AzureNative.Resources.ResourceGroup(prefix, new AzureNative.Resources.ResourceGroupArgs
    {
        ResourceGroupName = prefix
    });

    var jimmyUser = await AzureAD.GetUser.InvokeAsync(new AzureAD.GetUserArgs
    {
        UserPrincipalName = "jimmy.bogard_gmail.com#EXT#@jimmybogardgmail.onmicrosoft.com"
    });

    var eventGridResources = new EventGridResources(prefix, resourceGroup, jimmyUser);

    var eventHubNamespace = new AzureNative.EventHub.Namespace($"{prefix}-example-namespace",
        new AzureNative.EventHub.NamespaceArgs
        {
            ResourceGroupName = resourceGroup.Name,
            Sku = new AzureNative.EventHub.Inputs.SkuArgs
            {
                Name = AzureNative.EventHub.SkuName.Basic,
                Tier = AzureNative.EventHub.SkuTier.Basic,
            }
        });

    var eventHub = new AzureNative.EventHub.EventHub($"{prefix}-weather-forecast",
        new AzureNative.EventHub.EventHubArgs
        {
            NamespaceName = eventHubNamespace.Name,
            ResourceGroupName = resourceGroup.Name,
            PartitionCount = 2,
            MessageRetentionInDays = 1
        });

    var eventHubJimmySenderRoleAssignment = new AzureNative.Authorization.RoleAssignment(
        $"{prefix}-event-hub-data-sender-jimmy",
        new AzureNative.Authorization.RoleAssignmentArgs
        {
            PrincipalId = jimmyUser.ObjectId,
            PrincipalType = "User",
            Scope = eventHub.Id,
            // This is "Azure Event Hubs Data Sender" role
            RoleDefinitionId =
                "/providers/Microsoft.Authorization/roleDefinitions/2b629674-e913-4c01-ae53-ef4638d8f975"
        });


    return new Dictionary<string, object?>
    {
        { "ExampleTopicEndpoint", eventGridResources.ExampleTopicEndpoint },
        { "ExampleTopicId", eventGridResources.ExampleTopicId },
        { "ServiceBusName", eventGridResources.ServiceBusName },
        { "ServiceBusEndpoint", eventGridResources.ServiceBusEndpoint },
        { "ServiceBusQueue", eventGridResources.ServiceBusQueue },
        { "EventHubNamespaceName", eventHubNamespace.Name },
        { "EventHubName", eventHub.Name },
    };
});