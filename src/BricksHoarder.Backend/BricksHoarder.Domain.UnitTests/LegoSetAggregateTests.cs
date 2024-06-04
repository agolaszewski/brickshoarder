using BricksHoarder.Domain.LegoSet;
using BricksHoarder.Events;
using BricksHoarder.Websites.Scrappers.Lego;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace BricksHoarder.Domain.UnitTests
{
    public class LegoSetAggregateTests
    {
        private readonly LegoSetAggregate _legoSetAggregate = new()
        {
            Context = new ServiceCollection().BuildServiceProvider()
        };

        [Fact]
        public void when_response_of_lego_scrapper_is_different_than_the_current_state_of_lego_set_aggregate_and_the_current_state_is_unknown_then_the_lego_set_aggregate_is_new_for_the_system()
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
        public void when_response_of_lego_scrapper_is_unknown_and_current_state_is_unknown_then_the_lego_set_aggregate_has_unknown_state_and_lego_set_is_discontinued_and_no_longer_available_on_lego_website()
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
        public void when_response_of_lego_scrapper_is_unknown_and_current_state_is_different_than_unknown_that_means_there_is_some_error_on_lego_site_or_lego_set_is_discontinued_and_no_longer_available_on_lego_website()
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
        public void when_response_of_lego_scrapper_is_different_than_the_current_state_of_lego_set_aggregate_then_the_lego_set_aggregate_has_changed()
        {
            //When response of LegoScrapper is different than the current state of LegoSetAggregate than the LegoSetAggregate has changed

            // Arrange
            var response = new LegoScrapperResponse("10", "Test", Availability.Available, "123", "1", "http://example/123", null, false);

            // Act
            var result = _legoSetAggregate.HasChanged(response);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void when_response_of_lego_scrapper_is_different_than_the_current_state_of_lego_set_aggregate_then_the_lego_set_aggregate_should_be_updated_with_new_data()
        {
            //When response of LegoScrapper is different than the current state of LegoSetAggregate than the LegoSetAggregate should be updated with new data

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
        public void when_response_of_lego_scrapper_is_available_then_the_lego_set_aggregate_should_add_lego_set_is_available_event()
        {
            //When response of LegoScrapper is Available than the LegoSetAggregate should add LegoSetIsAvailable event

            // Arrange
            var response = new LegoScrapperResponse("10", "Test", Availability.Available, "123", "1", "http://example/123", null, false);
            var date = System.DateTime.Now;

            // Act
            _legoSetAggregate.CheckAvailability(response, date);

            // Assert
            _legoSetAggregate.Events.Should().ContainSingle(e => e.Event is LegoSetIsAvailable);
        }

        [Fact]
        public void when_response_of_lego_scrapper_is_awaiting_then_the_lego_set_aggregate_should_add_lego_set_to_be_released_event()
        {
            // Arrange
            var response = new LegoScrapperResponse("10", "Test", Availability.Awaiting, "123", "1", "http://example/123", System.DateTime.Now.AddDays(10), false);
            var date = System.DateTime.Now;

            // Act
            _legoSetAggregate.CheckAvailability(response, date);

            // Assert
            _legoSetAggregate.Events.Should().ContainSingle(e => e.Event is LegoSetToBeReleased);
        }

        [Fact]
        public void when_response_of_lego_scrapper_is_pending_then_the_lego_set_aggregate_should_add_lego_set_pending_event()
        {
            // Arrange
            var response = new LegoScrapperResponse("10", "Test", Availability.Pending, "123", "1", "http://example/123", System.DateTime.Now.AddDays(10), false);
            var date = System.DateTime.Now;

            // Act
            _legoSetAggregate.CheckAvailability(response, date);

            // Assert
            _legoSetAggregate.Events.Should().ContainSingle(e => e.Event is LegoSetPending);
        }

        [Fact]
        public void when_response_of_lego_scrapper_is_running_out_then_the_lego_set_aggregate_should_add_lego_set_running_out_event()
        {
            // Arrange
            var response = new LegoScrapperResponse("10", "Test", Availability.RunningOut, "123", "1", "http://example/123", null, false);
            var date = System.DateTime.Now;

            // Act
            _legoSetAggregate.CheckAvailability(response, date);

            // Assert
            _legoSetAggregate.Events.Should().ContainSingle(e => e.Event is LegoSetRunningOut);
        }

        [Fact]
        public void when_response_of_lego_scrapper_is_temporarily_unavailable_then_the_lego_set_aggregate_should_add_lego_set_temporarily_unavailable_event()
        {
            // Arrange
            var response = new LegoScrapperResponse("10", "Test", Availability.TemporarilyUnavailable, "123", "1", "http://example/123", null, false);
            var date = System.DateTime.Now;

            // Act
            _legoSetAggregate.CheckAvailability(response, date);

            // Assert
            _legoSetAggregate.Events.Should().ContainSingle(e => e.Event is LegoSetTemporarilyUnavailable);
        }

        [Fact]
        public void when_response_of_lego_scrapper_is_discontinued_then_the_lego_set_aggregate_should_add_lego_set_no_longer_for_sale_event()
        {
            // Arrange
            var response = new LegoScrapperResponse("10", "Test", Availability.Discontinued, "123", "1", "http://example/123", null, false);
            var date = System.DateTime.Now;

            // Act
            _legoSetAggregate.CheckAvailability(response, date);

            // Assert
            _legoSetAggregate.Events.Should().ContainSingle(e => e.Event is LegoSetNoLongerForSale);
        }
    }
}