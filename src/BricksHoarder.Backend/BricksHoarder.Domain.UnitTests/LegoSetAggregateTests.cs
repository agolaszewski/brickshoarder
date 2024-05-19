using System;
using BricksHoarder.Domain.LegoSet;
using BricksHoarder.Domain.SetsCollection;
using BricksHoarder.Events;
using BricksHoarder.Websites.Scrappers.Lego;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BricksHoarder.Domain.UnitTests
{
    public class LegoSetAggregateTests
    {
        private readonly LegoSetAggregate _legoSetAggregate = new()
        {
            Context = new ServiceCollection().BuildServiceProvider()
        };

        [Fact]
        public void CheckIfLegoSetIsNewForSystem()
        {
            //When response of LegoScrapper is different than the current state of LegoSetAggregate and the current state is unknown than the LegoSetAggregate is new for the system

            // Arrange
            var response = new LegoScrapperResponse("10", "Test", Availability.Available, "123", "1", "http://example/123", null, false);

            // Act
            var result = _legoSetAggregate.IsNewForSystem(response);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void CheckIfLegoSetHasUnknownStateAndUnknownAvailability()
        {
            //When response of LegoScrapper is unknown and current state is unknown than the LegoSetAggregate has unknown state and lego set is discontinued and no longer available on lego website.

            // Arrange
            var response = new LegoScrapperResponse("10", "Test", Availability.Unknown, "123", "1", "http://example/123", null, false);

            // Act
            var result = _legoSetAggregate.IsNewForSystem(response);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void HasUnknownState_ShouldReturnTrue_WhenResponseIsUnknownAndCurrentStateIsUnknown()
        {
            //When response of LegoScrapper is unknown and current state is different than unknown that means there is some error on Lego site OR Lego set is discontinued and no longer available on lego website.

            // Arrange
            _legoSetAggregate.Apply(new NewLegoSetDiscovered("10", "Test", LegoSetAvailability.Available, 10, 5, null, false));
            var response = new LegoScrapperResponse("10", "Test", Availability.Unknown, "123", "1", "http://example/123", null, false);

            // Act
            var result = _legoSetAggregate.HasUnknownState(response);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void HasChanged_ShouldReturnTrue_WhenResponseDiffersFromCurrentState()
        {
            // Arrange
            var response = new LegoScrapperResponse("10", "Test", Availability.Available, "123", "1", "http://example/123", null, false);

            // Act
            var result = _legoSetAggregate.HasChanged(response);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Update_ShouldAddLegoSetUpdatedEvent_WhenResponseDiffersFromCurrentState()
        {
            // Arrange
            _legoSetAggregate.Apply(new NewLegoSetDiscovered("10", "Test", LegoSetAvailability.Available, 10, 5, null, false));
            var response = new LegoScrapperResponse("10", "Test10", Availability.Awaiting, "123", "1", "http://example/123", null, false);

            // Act
            _legoSetAggregate.Update(response);

            // Assert
            _legoSetAggregate.Availability.Should().Be(LegoSetAvailability.Awaiting);
            _legoSetAggregate.MaxQuantity.Should().Be(1);
            _legoSetAggregate.Price.Should().Be(123);
        }

        [Fact]
        public void AddLegoSetToBeReleasedEvent_WhenResponseAvailabilityIsAwaiting()
        {
            // Arrange
            var aggregate = new LegoSetAggregate();
            var response = new LegoScrapperResponse("10", "Test", Availability.Available, "123", "1", "http://example/123", null, false);
            var date = System.DateTime.Now;

            // Act
            aggregate.CheckAvailability(response, date);

            // Assert
            aggregate.Events.Should().ContainSingle(e => e is LegoSetToBeReleased);
        }
    }
}