using System;
using System.Threading.Tasks;
using BricksHoarder.Cloud.Azure.Infrastructure.Generator.Stacks;
using Deployment = Pulumi.Deployment;

namespace BricksHoarder.Cloud.Azure.Infrastructure.Generator;

internal class Program
{
    private static Task<int> Main()
    {
        var stackName = Environment.GetEnvironmentVariable("PULUMI_STACK");

        return stackName switch
        {
            "Dev" => Deployment.RunAsync<Dev>(),
            "Production" => Deployment.RunAsync<Production>(),
            _ => Task.FromResult(0)
        };
    }
}