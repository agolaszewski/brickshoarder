using System;
using System.Collections.Generic;
using Pulumi;
using Pulumi.AzureNative.Consumption;
using Pulumi.AzureNative.Consumption.Inputs;

namespace BricksHoarder.Cloud.Azure.Infrastructure.Generator.Resources
{
    public class AzureBudgetForecasted : ComponentResource
    {
        public AzureBudgetForecasted(string subscription, string name, double amount, string email) : base("Custom:Consumption:Budget:AzureBudgetForecasted", name, null, null)
        {
            _ = new Budget($"Budget.{name}", new BudgetArgs
            {
                BudgetName = name,
                Scope = $"/subscriptions/{subscription}",
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
                            Threshold = 100,
                            Operator = OperatorType.GreaterThanOrEqualTo,
                            ThresholdType = ThresholdType.Forecasted,
                            ContactEmails = new[] { email }
                        }
                    }
                }
            });
        }
    }
}