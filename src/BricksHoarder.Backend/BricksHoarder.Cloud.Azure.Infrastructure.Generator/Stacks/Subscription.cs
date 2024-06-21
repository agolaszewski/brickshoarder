using BricksHoarder.Cloud.Azure.Infrastructure.Generator.Resources;
using Microsoft.Extensions.Configuration;
using Pulumi;

namespace BricksHoarder.Cloud.Azure.Infrastructure.Generator.Stacks
{
    internal class Subscription : Stack
    {
        public Subscription()
        {
            var config = new Config();
            var subscription = config.Require("subscription");
            var email = config.Require("email");

            _ = new AzureBudget(subscription,"biedactwo", 5, email);
            _ = new AzureBudget(subscription, "bogactwo", 10, email);
            _ = new AzureBudget(subscription, "o-kurwa", 20, email);
            _ = new AzureBudgetForecasted(subscription, "o-kurwa-zprzyszlosci", 30, email);
        }
    }
}