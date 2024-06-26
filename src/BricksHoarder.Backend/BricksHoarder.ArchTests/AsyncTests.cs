using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using System.Threading.Tasks;
using BricksHoarder.Api;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BricksHoarder.Arch.Tests
{
    public class AsyncTests
    {
        private static readonly Architecture Architecture =
            new ArchLoader().LoadAssemblyIncludingDependencies(typeof(Pointer).Assembly)
                .Build();

        private static readonly IObjectProvider<MethodMember> BricksHoarderMethods =
            MethodMembers()
                .That()
                .HaveFullNameContaining("BricksHoarder")
                .And()
                .DoNotHaveFullNameContaining("BricksHoarder.AzureCloud.ServiceBus.CommandConsumer")
                .And()
                .DoNotHaveFullNameContaining("BricksHoarder.AzureCloud.ServiceBus.EventConsumer");

        [Fact]
        public void Async_Methods_Should_Ends_With_Async()
        {
            var asyncMethods = MethodMembers().That()
                .HaveReturnType(typeof(Task))
                .Or()
                .HaveReturnType(typeof(Task<>));

            asyncMethods
                .And()
                .Are(BricksHoarderMethods)
                .Should()
                .HaveNameContaining("Async")
                .Check(Architecture);
        }
    }
}