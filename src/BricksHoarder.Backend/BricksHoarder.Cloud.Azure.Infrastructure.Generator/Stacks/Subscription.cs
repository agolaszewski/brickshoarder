using BricksHoarder.Cloud.Azure.Infrastructure.Generator.Resources;
using Pulumi;

namespace BricksHoarder.Cloud.Azure.Infrastructure.Generator.Stacks
{
    internal class Subscription : Stack
    {
        public Subscription()
        {
            var config = new Config();
            var clientConfig = Output.Create(Pulumi.AzureNative.Authorization.GetClientConfig.InvokeAsync());
            var subscriptionId = clientConfig.Apply(x => x.SubscriptionId);
            var email = config.Require("email");

            _ = new AzureBudget(subscriptionId, "biedactwo", 5, email);
            _ = new AzureBudget(subscriptionId, "bogactwo", 10, email);
            _ = new AzureBudget(subscriptionId, "o-kurwa", 20, email);
            _ = new AzureBudgetForecasted(subscriptionId, "o-kurwa-z-przyszlosci", 30, email);
        }
    }
}