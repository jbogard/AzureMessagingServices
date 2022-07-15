using Pulumi;
using AzureNative = Pulumi.AzureNative;
using AzureAD = Pulumi.AzureAD;

namespace Infrastructure;

public class EventHubResources
{
    public EventHubResources(string prefix,
        AzureNative.Resources.ResourceGroup resourceGroup,
        AzureAD.GetUserResult jimmyUser)
    {
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

        var eventHubJimmyReceiverRoleAssignment = new AzureNative.Authorization.RoleAssignment(
            $"{prefix}-event-hub-data-receiver-jimmy",
            new AzureNative.Authorization.RoleAssignmentArgs
            {
                PrincipalId = jimmyUser.ObjectId,
                PrincipalType = "User",
                Scope = eventHub.Id,
            // This is "Azure Event Hubs Data Receiver" role
                RoleDefinitionId =
                    "/providers/Microsoft.Authorization/roleDefinitions/a638d3c7-ab3a-418d-83e6-5f17a39d4fde"
            });

        var eventHubJimmyOwnerRoleAssignment = new AzureNative.Authorization.RoleAssignment(
            $"{prefix}-event-hub-data-owner-jimmy",
            new AzureNative.Authorization.RoleAssignmentArgs
            {
                PrincipalId = jimmyUser.ObjectId,
                PrincipalType = "User",
                Scope = eventHub.Id,
            // This is "Azure Event Hubs Data Owner" role
                RoleDefinitionId =
                    "/providers/Microsoft.Authorization/roleDefinitions/f526a384-b230-433a-b45c-95f59c4a2dec"
            });

        EventHubNamespaceName = eventHubNamespace.Name;
        EventHubName = eventHub.Name;
    }

    public Output<string> EventHubName { get; set; }

    public Output<string> EventHubNamespaceName { get; set; }
}