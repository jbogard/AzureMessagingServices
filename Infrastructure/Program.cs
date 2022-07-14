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


    return new Dictionary<string, object?>
    {
        { "ExampleTopicEndpoint", eventGridResources.ExampleTopicEndpoint },
        { "ExampleTopicId", eventGridResources.ExampleTopicId },
        { "ServiceBusName", eventGridResources.ServiceBusName },
        { "ServiceBusEndpoint", eventGridResources.ServiceBusEndpoint },
        { "ServiceBusQueue", eventGridResources.ServiceBusQueue },
    };
});