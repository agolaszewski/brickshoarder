using System;
using System.Collections.Generic;
using Pulumi;
using Pulumi.AzureNative.App.Inputs;
using Pulumi.AzureNative.Consumption;
using Pulumi.AzureNative.Consumption.Inputs;
using Pulumi.AzureNative.KeyVault.Inputs;

namespace BricksHoarder.Cloud.Azure.Infrastructure.Generator.Resources
{
    public class AzureBudget : ComponentResource
    {
        public AzureBudget(Output<string> subscription, string name, double amount, string email) : base("Custom:Consumption:Budget:AzureBudget", name, null, null)
        {
            _ = new Budget($"Budget.{name}", new BudgetArgs
            {
                BudgetName = name,
                Scope = Output.Format($"/subscriptions/{subscription}"),
                Amount = amount,
                TimeGrain = TimeGrainType.Monthly,
                Category = CategoryType.Cost,
                TimePeriod = new BudgetTimePeriodArgs
                {
                    StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToShortDateString(),
                },
                Notifications = new Dictionary<string, NotificationArgs>
                {
                    {
                        "Critical", new NotificationArgs
                        {
                            Enabled = true,
                            Threshold = 90,
                            Operator = OperatorType.GreaterThanOrEqualTo,
                            ThresholdType = ThresholdType.Actual,
                            ContactEmails = new[] { email }
                        }
                    }
                }
            });
        }
    }
}