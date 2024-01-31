using System.IO;
using System.Threading.Tasks;
using BricksHoarder.Cloud.Azure.Infrastructure.Generator.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Pulumi;
using Pulumi.AzureNative.Resources;

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

            var serviceBusNamespace = new StandardServiceBusNamespace("Default", "dev", resourceGroup);
            ServiceBusEndpoint = serviceBusNamespace.ServiceBusEndpoint;
            SharedAccessKey = serviceBusNamespace.SharedAccessKey;
            SharedAccessKeyName = serviceBusNamespace.SharedAccessKeyName;
            ServiceBusConnectionString = serviceBusNamespace.ServiceBusConnectionString;

            Output.All(ServiceBusEndpoint, SharedAccessKey, SharedAccessKeyName, ServiceBusConnectionString).Apply(_ =>
            {
                string json = File.ReadAllText("..//BricksHoarder.Functions//dev.settings.json");

                JObject jObject = Newtonsoft.Json.JsonConvert.DeserializeObject(json) as JObject;

                JToken serviceBusConnectionStringToken = jObject!.SelectToken("Values.ServiceBusConnectionString")!;
                serviceBusConnectionStringToken.Replace(ServiceBusConnectionString.Convert());

                JToken azureServiceBusEndpoint = jObject!.SelectToken("AzureServiceBus.Endpoint")!;
                azureServiceBusEndpoint.Replace(ServiceBusEndpoint.Convert().Replace("https://",string.Empty));

                JToken azureServiceBusSharedAccessKeyName = jObject!.SelectToken("AzureServiceBus.SharedAccessKeyName")!;
                azureServiceBusSharedAccessKeyName.Replace(SharedAccessKeyName.Convert());

                JToken azureServiceBusSharedAccessKey = jObject!.SelectToken("AzureServiceBus.SharedAccessKey")!;
                azureServiceBusSharedAccessKey.Replace(SharedAccessKey.Convert());

                string updatedJsonString = jObject.ToString();
                File.WriteAllText("..//BricksHoarder.Functions//dev.settings.json", updatedJsonString);

                return Task.CompletedTask;
            });


        }

        [Output]
        public Output<string> ServiceBusEndpoint { get; set; }

        [Output]
        public Output<string> SharedAccessKey { get; set; }

        [Output]
        public Output<string> SharedAccessKeyName { get; set; }

        [Output]
        public Output<string> ServiceBusConnectionString { get; set; }
    }
}