using System.Threading.Tasks;
using Deployment = Pulumi.Deployment;

namespace BricksHoarder.Cloud.Azure.Infrastructure.Generator;

internal class Program
{
    private static Task<int> Main() => Deployment.RunAsync<MyStack>();
}