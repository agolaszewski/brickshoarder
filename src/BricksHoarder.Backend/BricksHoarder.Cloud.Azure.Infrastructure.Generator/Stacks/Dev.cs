using System.Collections.Generic;
using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.ServiceBus;
using Pulumi.AzureNative.ServiceBus.Inputs;

namespace BricksHoarder.Cloud.Azure.Infrastructure.Generator.Stacks
{
    internal class Dev : Stack
    {
        public Dev()
        {
            var resourceGroup = new ResourceGroup("ResourceGroup", new ResourceGroupArgs
            {
                ResourceGroupName = "rg-brickshoarder-dev"
            });

            var serviceBusNamespace = new StandardServiceBusNamespace("Default", resourceGroup);
            ServiceBusEndpoint = serviceBusNamespace.ServiceBusEndpoint;
        }

        [Output]
        public Output<string> ServiceBusEndpoint { get; set; }
    }

    public class StandardServiceBusNamespace : ComponentResource
    {
        [Output]
        public Output<string> ServiceBusEndpoint { get; private set; }

        public StandardServiceBusNamespace(string name, ResourceGroup resourceGroup) : base("Custom:ServiceBus:Namespace", name, null, null)
        {
            var serviceBusNamespace = new Namespace($"ServiceBus.Namespace.{name}", new NamespaceArgs
            {
                ResourceGroupName = resourceGroup.Name,
                Sku = new SBSkuArgs()
                {
                    Name = SkuName.Standard,
                    Tier = SkuTier.Standard
                },
                NamespaceName = "sb-brickshoarder-dev"
            });

            var serviceBusNamespaceNamespaceAuthorizationRule = new NamespaceAuthorizationRule($"ServiceBus.Namespace.NamespaceAuthorizationRule.{name}", new NamespaceAuthorizationRuleArgs()
            {
                NamespaceName = serviceBusNamespace.Name,
                AuthorizationRuleName = "DefaultSharedAccessPolicy",
                ResourceGroupName = resourceGroup.Name,
                Rights =
                {
                    AccessRights.Manage,
                    AccessRights.Listen,
                    AccessRights.Send
                }
            });

            ServiceBusEndpoint = serviceBusNamespace.ServiceBusEndpoint;
            //SharedAccessKeyName = serviceBusNamespaceNamespaceAuthorizationRule.Name;

            var output = Output.All(serviceBusNamespaceNamespaceAuthorizationRule.Name).Apply(async x =>
            {
                var namespaceKeys = await ListNamespaceKeys.InvokeAsync(new ListNamespaceKeysArgs()
                {
                    AuthorizationRuleName = serviceBusNamespaceNamespaceAuthorizationRule.Name.Convert(),
                    NamespaceName = serviceBusNamespace.Name.Convert(),
                    ResourceGroupName = resourceGroup.Name.Convert()
                });
                return (namespaceKeys.PrimaryConnectionString, namespaceKeys.PrimaryKey);
            });
            //ServiceBusConnectionString = output.Apply(x => x.PrimaryConnectionString);
            //SharedAccessKey = output.Apply(x => x.PrimaryKey);

            RegisterOutputs(new Dictionary<string, object?>
            {
                { "ServiceBusEndpoint", ServiceBusEndpoint }
            });
        }
    }
}