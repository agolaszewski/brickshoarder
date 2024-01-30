using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pulumi;
using Pulumi.AzureNative.Resources;

namespace BricksHoarder.Cloud.Azure.Infrastructure.Generator.Stacks
{
    internal class Production : Stack
    {
        public Production()
        {
            var services = new ServiceCollection();
            var config = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .Build();

            var resourceGroup = new ResourceGroup("ResourceGroup", new ResourceGroupArgs
            {
                ResourceGroupName = "rg-brickshoarder-prd"
            });
        }
    }
}