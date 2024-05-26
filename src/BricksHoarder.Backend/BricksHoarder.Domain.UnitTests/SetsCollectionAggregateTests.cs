using BricksHoarder.Domain.SetsCollection;
using BricksHoarder.Events;
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
            public void when_set_data_has_changed_the_sets_collection_aggregate_should_return_true()
            {
                //When set data has changed, the SetsCollectionAggregate should return true

                // Arrange
                var set = new LegoSetsListAsyncResponse.Result("1", "Test Set", 2024, 1, 2137, null, null, new System.DateTime(2024, 1, 1, 1, 1, 1));

                // Act
                bool hasChange = _setsCollectionAggregate.HasChanged(set);

                // Assert
                hasChange.Should().BeTrue();
                _setsCollectionAggregate.Collection.Any(x => x.SetId == "1").Should().BeTrue();
            }

            [Fact]
            public void when_set_data_has_not_changed_the_sets_collection_aggregate_should_return_false()
            {
                //When set data has not changed, the SetsCollectionAggregate should return false

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
            public void when_set_data_has_changed_the_sets_collection_aggregate_should_be_updated_with_new_data()
            {
                //When set data has changed, the SetsCollectionAggregate should be updated with new data

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

            [Fact]
            public void when_set_is_released_it_should_be_added_to_the_collection()
            {
                // Arrange
                var setReleasedEvent = new SetReleased("123", System.DateTime.Now);

                // Act
                _setsCollectionAggregate.Apply(setReleasedEvent);

                // Assert
                _setsCollectionAggregate.Collection.Should().ContainSingle(s => s.SetId == "123");
            }

            [Fact]
            public void when_set_details_are_changed_it_should_update_the_set()
            {
                // Arrange
                var setReleasedEvent = new SetReleased("123", System.DateTime.Now.AddDays(-1));
                _setsCollectionAggregate.Apply(setReleasedEvent);
                var setDetailsChangedEvent = new SetDetailsChanged("123", System.DateTime.Now);

                // Act
                _setsCollectionAggregate.Apply(setDetailsChangedEvent);

                // Assert
                var set = _setsCollectionAggregate.Collection.Single(s => s.SetId == "123");
                set.LastModifiedDate.Should().Be(setDetailsChangedEvent.LastModifiedDate);
            }

            [Fact]
            public void when_checking_if_set_has_changed_and_set_does_not_exist_it_should_return_true()
            {
                // Arrange
                var apiSet = new LegoSetsListAsyncResponse.Result("1", "Test Set", 2024, 1, 2137, null, null, new System.DateTime(2024, 1, 1, 1, 1, 1));

                // Act
                var hasChanged = _setsCollectionAggregate.HasChanged(apiSet);

                // Assert
                hasChanged.Should().BeTrue();
            }

            [Fact]
            public void when_checking_if_set_has_changed_and_set_exists_but_last_modified_date_is_older_it_should_return_true()
            {
                // Arrange
                var setReleasedEvent = new SetReleased("123", DateTimeHelper.Today.AddDays(-1));
                _setsCollectionAggregate.Apply(setReleasedEvent);
                var apiSet = new LegoSetsListAsyncResponse.Result("123", "Test Set", 2024, 1, 2137, null, null, DateTimeHelper.Today);

                // Act
                var hasChanged = _setsCollectionAggregate.HasChanged(apiSet);

                // Assert
                hasChanged.Should().BeTrue();
            }

            [Fact]
            public void when_checking_if_set_has_changed_and_set_exists_and_last_modified_date_is_same_it_should_return_false()
            {
                // Arrange
                var date = System.DateTime.Now;
                var setReleasedEvent = new SetReleased("123", date);
                _setsCollectionAggregate.Apply(setReleasedEvent);
                var apiSet = new LegoSetsListAsyncResponse.Result("1", "Test Set", 2024, 1, 2137, null, null, new System.DateTime(2024, 1, 1, 1, 1, 1));

                // Act
                var hasChanged = _setsCollectionAggregate.HasChanged(apiSet);

                // Assert
                hasChanged.Should().BeFalse();
            }

        }
    }
}