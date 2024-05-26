using BricksHoarder.Domain.RebrickableSet;
using BricksHoarder.Events;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Rebrickable.Api;

namespace BricksHoarder.Domain.UnitTests
{
    public class RebrickableSetAggregateTests
    {
        private readonly RebrickableSetAggregate _rebrickableSetAggregate = new()
        {
            Context = new ServiceCollection().BuildServiceProvider()
        };

        [Fact]
        public void when_set_data_has_changed_the_rebrickable_set_aggregate_should_return_true()
        {
            //When set data has changed, the RebrickableSetAggregate should return true

            // Arrange
            _rebrickableSetAggregate.Apply(new RebrickableSetDataSynced("123", "TestOld", 1, 2004, 1, "1231"));
            var apiSet = new LegoSetsReadAsyncResponse("123", "Test", 2024, 2, 2, "Test", "test", DateTimeHelper.Today);

            // Act
            var result = _rebrickableSetAggregate.HasChanged(apiSet);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void when_set_data_has_not_changed_the_rebrickable_set_aggregate_should_return_false()
        {
            //When set data has not changed, the RebrickableSetAggregate should return false

            // Arrange
            _rebrickableSetAggregate.Apply(new RebrickableSetDataSynced("123", "Test", 1, 2024, 1, "1231"));
            var apiSet = new LegoSetsReadAsyncResponse("123", "Test", 2024, 1, 1, "1231", "test", DateTimeHelper.Today);

            // Act
            var result = _rebrickableSetAggregate.HasChanged(apiSet);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void when_set_data_has_changed_the_rebrickable_set_aggregate_should_be_updated_with_new_data()
        {
            //When set data has changed, the RebrickableSetAggregate should be updated with new data

            // Arrange
            var apiSet = new LegoSetsReadAsyncResponse("123", "Test", 2024, 1, 1, "1231", "test", DateTimeHelper.Today);

            // Act
            _rebrickableSetAggregate.SetData(apiSet);

            // Assert
            _rebrickableSetAggregate.Name.Should().Be("Test");
            _rebrickableSetAggregate.NumParts.Should().Be(1);
            _rebrickableSetAggregate.SetImgUrl.Should().Be("1231");
            _rebrickableSetAggregate.ThemeId.Should().Be(1);
            _rebrickableSetAggregate.Year.Should().Be(2024);
        }

        [Fact]
        public void when_new_minifigures_are_added_the_rebrickable_set_aggregate_should_add_new_minifigures()
        {
            //When new minifigures are added, the RebrickableSetAggregate should add new minifigures

            // Arrange
            var results = new List<LegoSetsMinifigsListAsyncResponse.Result>
                {
                    new(1, "minifigureId1", "minifigureName1", 1, "minifigureImageUrl1"),
                    new(2, "minifigureId2", "minifigureName2", 1, "minifigureImageUrl2"),
                };

            // Act
            _rebrickableSetAggregate.SetMinifigureData(results);

            // Assert
            _rebrickableSetAggregate.Minifigures.Count.Should().Be(2);
            var minifigure1 = _rebrickableSetAggregate.Minifigures[0];
            minifigure1.Id.Should().Be(1);
            minifigure1.Name.Should().Be("minifigureName1");
            minifigure1.Quantity.Should().Be(1);
            minifigure1.ImageUrl.Should().Be("minifigureImageUrl1");

            var minifigure2 = _rebrickableSetAggregate.Minifigures[1];
            minifigure2.Id.Should().Be(2);
            minifigure2.Name.Should().Be("minifigureName2");
            minifigure2.Quantity.Should().Be(1);
            minifigure2.ImageUrl.Should().Be("minifigureImageUrl2");
        }

        [Fact]
        public void when_new_minifigures_are_added_the_rebrickable_set_aggregate_should_add_new_minifigures_and_remove_old_ones()
        {
            //When new minifigures are added, the RebrickableSetAggregate should add new minifigures and remove old ones

            // Arrange
            _rebrickableSetAggregate.Apply(new RebrickableMinifigureAddedToSet("123", 123, "mini1", 1, ""));

            var results = new List<LegoSetsMinifigsListAsyncResponse.Result>
                {
                    new(1, "minifigureId1", "minifigureName1", 1, "minifigureImageUrl1"),
                    new(2, "minifigureId2", "minifigureName2", 1, "minifigureImageUrl2"),
                };
            // Act
            _rebrickableSetAggregate.SetMinifigureData(results);

            // Assert
            _rebrickableSetAggregate.Minifigures.Count.Should().Be(2);
        }

        [Fact]
        public void when_same_minifigure_is_added_again_with_different_data_the_rebrickable_set_aggregate_should_be_updated_with_new_data()
        {
            //When same minifigure is added again with different data, the RebrickableSetAggregate should be updated with new data

            // Arrange
            _rebrickableSetAggregate.Apply(new RebrickableMinifigureAddedToSet("123", 123, "mini1", 1, ""));

            var results = new List<LegoSetsMinifigsListAsyncResponse.Result>
            {
                new(123, "123", "minifigureName1", 5, "minifigureImageUrl1"),
            };

            // Act
            _rebrickableSetAggregate.SetMinifigureData(results);

            // Assert
            _rebrickableSetAggregate.Minifigures.Count.Should().Be(1);
            _rebrickableSetAggregate.Minifigures[0].Name.Should().Be("minifigureName1");
            _rebrickableSetAggregate.Minifigures[0].Quantity.Should().Be(5);
            _rebrickableSetAggregate.Minifigures[0].ImageUrl.Should().Be("minifigureImageUrl1");
        }

        [Fact]
        public void when_same_minifigure_is_added_again_with_same_data_the_mini_figure_data_should_not_be_changed()
        {
            //When same minifigure is added again with same data, the mini figure data should not be changed

            // Arrange
            _rebrickableSetAggregate.Apply(new RebrickableMinifigureAddedToSet("123", 123, "minifigureId1", 1, "minifigureImageUrl1"));

            var results = new List<LegoSetsMinifigsListAsyncResponse.Result>
                {
                    new(123, "minifigureId1", "minifigureName1", 1, "minifigureImageUrl1"),
                };
            // Act

            _rebrickableSetAggregate.SetMinifigureData(results);

            // Assert
            _rebrickableSetAggregate.Minifigures.Count.Should().Be(1);
        }

        [Fact]
        public void when_minifigure_is_added_it_should_be_added_to_the_list()
        {
            // Arrange
            var minifigureAddedEvent = new RebrickableMinifigureAddedToSet("123", 1, "Test", 1, "http://example.com");

            // Act
            _rebrickableSetAggregate.Apply(minifigureAddedEvent);

            // Assert
            _rebrickableSetAggregate.Minifigures.Should().ContainSingle(m => m.Id == 1);
            var minifigure = _rebrickableSetAggregate.Minifigures.Single(m => m.Id == 1);
            minifigure.Name.Should().Be("Test");
            minifigure.Quantity.Should().Be(1);
            minifigure.ImageUrl.Should().Be("http://example.com");
            minifigure.Id.Should().Be(1);
        }

        [Fact]
        public void when_minifigure_is_deleted_it_should_be_removed_from_the_list()
        {
            // Arrange
            var minifigureAddedEvent = new RebrickableMinifigureAddedToSet("123", 1, "Test", 1, "http://example.com");
            _rebrickableSetAggregate.Apply(minifigureAddedEvent);
            var minifigureDeletedEvent = new RebrickableMinifigureDeletedFromSet(1, "123");

            // Act
            _rebrickableSetAggregate.Apply(minifigureDeletedEvent);

            // Assert
            _rebrickableSetAggregate.Minifigures.Should().BeEmpty();
        }

        [Fact]
        public void when_minifigure_data_is_synced_it_should_update_the_minifigure()
        {
            // Arrange
            var minifigureAddedEvent = new RebrickableMinifigureAddedToSet("123", 1, "Test", 1, "http://example.com");
            _rebrickableSetAggregate.Apply(minifigureAddedEvent);
            var minifigureSyncedEvent = new RebrickableMinifigureDataSynced("123", 1, "Updated", 2, "http://updated.com");

            // Act
            _rebrickableSetAggregate.Apply(minifigureSyncedEvent);

            // Assert
            var minifigure = _rebrickableSetAggregate.Minifigures.Single(m => m.Id == 1);
            minifigure.Name.Should().Be("Updated");
            minifigure.Quantity.Should().Be(2);
            minifigure.ImageUrl.Should().Be("http://updated.com");
            minifigure.Id.Should().Be(1);
        }

        [Fact]
        public void when_set_data_is_synced_it_should_update_the_set()
        {
            // Arrange
            var setDataSyncedEvent = new RebrickableSetDataSynced("123", "Updated", 2, 2000, 100, "http://updated.com");

            // Act
            _rebrickableSetAggregate.Apply(setDataSyncedEvent);

            // Assert
            _rebrickableSetAggregate.Name.Should().Be("Updated");
            _rebrickableSetAggregate.ThemeId.Should().Be(2);
            _rebrickableSetAggregate.Year.Should().Be(2000);
            _rebrickableSetAggregate.NumParts.Should().Be(100);
            _rebrickableSetAggregate.SetImgUrl.Should().Be("http://updated.com");
        }

    }
}