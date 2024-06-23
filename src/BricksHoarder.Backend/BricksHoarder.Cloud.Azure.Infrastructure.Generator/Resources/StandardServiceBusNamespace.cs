using System.Collections.Generic;
using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.ServiceBus;
using Pulumi.AzureNative.ServiceBus.Inputs;
using ResourceArgs = Pulumi.ResourceArgs;

namespace BricksHoarder.Cloud.Azure.Infrastructure.Generator.Resources
{
    public class StandardServiceBusNamespace : ComponentResource
    {
        [Output]
        public Output<string> ServiceBusEndpoint { get; set; }

        [Output]
        public Output<string> SharedAccessKeyName { get; set; }

        [Output]
        public Output<string> ServiceBusConnectionString { get; set; }

        [Output]
        public Output<string> SharedAccessKey { get; set; }

        public StandardServiceBusNamespace(string name, string env, ResourceGroup resourceGroup) : base("Custom:ServiceBus:Namespace", name, null, null)
        {
            var serviceBusNamespace = new Namespace($"ServiceBus.Namespace.{name}", new NamespaceArgs
            {
                ResourceGroupName = resourceGroup.Name,
                Sku = new SBSkuArgs()
                {
                    Name = SkuName.Standard,
                    Tier = SkuTier.Standard
                },
                NamespaceName = $"sb-brickshoarder-{env}"
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
            SharedAccessKeyName = serviceBusNamespaceNamespaceAuthorizationRule.Name;

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

            ServiceBusConnectionString = output.Apply(x => x.PrimaryConnectionString);
            SharedAccessKey = output.Apply(x => x.PrimaryKey);

            RegisterOutputs(new Dictionary<string, object?>
            {
                { "ServiceBusEndpoint", ServiceBusEndpoint },
                { "SharedAccessKeyName" , SharedAccessKeyName},
                { "ServiceBusConnectionString" , ServiceBusConnectionString},
                { "SharedAccessKey" , SharedAccessKey}
            });
        }
    }
}