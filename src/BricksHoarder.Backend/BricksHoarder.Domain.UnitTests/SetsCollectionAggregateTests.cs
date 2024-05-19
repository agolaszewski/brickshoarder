using BricksHoarder.Domain.SetsCollection;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Rebrickable.Api;

namespace BricksHoarder.Domain.UnitTests
{
    namespace BricksHoarder.Domain.UnitTests
    {
        public class SetsCollectionAggregateTests
        {
            private readonly SetsCollectionAggregate _setsCollectionAggregate = new()
            {
                Context = new ServiceCollection().BuildServiceProvider()
            };

            [Fact]
            public void AddNotExistingSet()
            {
                // Arrange
                var set = new LegoSetsListAsyncResponse.Result("1", "Test Set", 2024, 1, 2137, null, null, new System.DateTime(2024, 1, 1, 1, 1, 1));

                // Act
                bool hasChange = _setsCollectionAggregate.HasChanged(set);

                // Assert
                hasChange.Should().BeTrue();
                _setsCollectionAggregate.Collection.Any(x => x.SetId == "1").Should().BeTrue();
            }

            [Fact]
            public void AddExistingSet()
            {
                // Arrange
                var set1 = new LegoSetsListAsyncResponse.Result("1", "Test Set", 2024, 1, 2137, null, null, new System.DateTime(2024, 1, 1, 1, 1, 1));
                var set2 = new LegoSetsListAsyncResponse.Result("1", "Test Set", 2024, 1, 2137, null, null, new System.DateTime(2024, 1, 1, 1, 1, 1));

                // Act
                bool hasChange1 = _setsCollectionAggregate.HasChanged(set1);
                hasChange1.Should().BeTrue();

                bool hasChange2 = _setsCollectionAggregate.HasChanged(set2);
                hasChange2.Should().BeFalse();
                _setsCollectionAggregate.Collection.Count().Should().Be(1);
            }

            [Fact]
            public void ExistingSetWithLatestLastModifiedDt()
            {
                // Arrange
                var set1 = new LegoSetsListAsyncResponse.Result("1", "Test Set", 2024, 1, 2137, null, null, new System.DateTime(2024, 1, 1, 1, 1, 1));
                var set2 = new LegoSetsListAsyncResponse.Result("1", "Test Set", 2024, 1, 2137, null, null, new System.DateTime(2024, 2, 1, 1, 1, 1));

                // Act
                bool hasChange1 = _setsCollectionAggregate.HasChanged(set1);
                hasChange1.Should().BeTrue();

                bool hasChange2 = _setsCollectionAggregate.HasChanged(set2);
                hasChange2.Should().BeTrue();
                _setsCollectionAggregate.Collection.First(x => x.SetId == "1").LastModifiedDate.Should().Be(new System.DateTime(2024, 2, 1, 1, 1, 1));
            }
        }
    }
}