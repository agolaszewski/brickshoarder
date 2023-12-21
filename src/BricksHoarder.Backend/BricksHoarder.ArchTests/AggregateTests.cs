using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using BricksHoarder.Common.DDD.Aggregates;
using BricksHoarder.Domain;
using MessagePack;
using Xunit;

using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BricksHoarder.ArchTests
{
    [MessagePackObject]
    public class AggregateTests
    {
        private static readonly Architecture Architecture =
            new ArchLoader().LoadAssemblies(typeof(BricksHoarderDomainAssemblyPointer).Assembly, typeof(MessagePackObjectAttribute).Assembly).Build();

        [Fact]
        public void Aggregate_Should_End_With_Aggregate_And_Has_Attribute_MessagePackObjectAttribute()
        {
            Classes().That().AreAssignableTo(typeof(AggregateRoot<>)).Should()
                .HaveNameEndingWith("Aggregate")
                .AndShould().HaveAttributeWithArguments(typeof(MessagePackObjectAttribute), true)
                .Check(Architecture);
        }
    }
}