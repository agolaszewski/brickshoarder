using BricksHoarder.Domain.ThemesCollection;
using BricksHoarder.Events;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Rebrickable.Api;

namespace BricksHoarder.Domain.UnitTests
{
    public class ThemesCollectionAggregateTests
    {
        private readonly ThemesCollectionAggregate _themesCollectionAggregate = new()
        {
            Context = new ServiceCollection().BuildServiceProvider()
        };

        [Fact]
        public void when_adding_a_non_existing_theme_to_the_collection_then_it_should_be_added()
        {
            //When adding a non-existing theme to the collection than it should be added

            // Arrange

            var theme = new LegoThemesListAsyncResponse.Result(1, null, "Test Theme");

            // Act
            _themesCollectionAggregate.Add(theme);

            // Assert
            _themesCollectionAggregate.Collection.Any(x => x.ThemeId == 1).Should().BeTrue();
        }

        [Fact]
        public void when_adding_an_existing_theme_to_the_collection_does_not_add_it_again()
        {
            //When adding an existing theme to the collection does not add it again

            // Arrange
            var theme = new LegoThemesListAsyncResponse.Result(1, null, "Test Theme");

            // Act
            _themesCollectionAggregate.Add(theme);
            _themesCollectionAggregate.Add(theme);

            // Assert
            _themesCollectionAggregate.Collection.Count(x => x.ThemeId == 1).Should().Be(1);
        }

        [Fact]
        public void when_adding_a_theme_with_a_parent_to_the_collection_then_it_should_be_added()
        {
            // When adding a theme with a parent to the collection than it should be added
            
            // Arrange
            var parentTheme = new LegoThemesListAsyncResponse.Result(1, null, "Parent Theme");
            var theme = new LegoThemesListAsyncResponse.Result(2, parentTheme.Id, "Child Theme");

            // Act
            _themesCollectionAggregate.Add(parentTheme);
            _themesCollectionAggregate.Add(theme);

            // Assert
            _themesCollectionAggregate.Collection.Any(x => x.ThemeId == 2).Should().BeTrue();
            _themesCollectionAggregate.Collection.Any(x => x.ThemeId == 1).Should().BeTrue();
            _themesCollectionAggregate.Collection.First(x => x.ThemeId == 1).Children.Any(x => x.ThemeId == 2).Should().BeTrue();
        }

        [Fact]
        public void when_theme_is_released_it_should_be_added_to_the_collection()
        {
            // Arrange
            var themeReleasedEvent = new ThemeReleased(1, null, "Test");

            // Act
            _themesCollectionAggregate.Apply(themeReleasedEvent);

            // Assert
            _themesCollectionAggregate.Collection.Should().ContainSingle(t => t.ThemeId == 1);
        }

        [Fact]
        public void when_theme_with_parent_is_released_it_should_be_added_to_the_collection_and_to_parent_children()
        {
            // Arrange
            var parentThemeReleasedEvent = new ThemeReleased(1, null, "Parent");
            _themesCollectionAggregate.Apply(parentThemeReleasedEvent);
            var themeReleasedEvent = new ThemeReleased(2, 1, "Child");

            // Act
            _themesCollectionAggregate.Apply(themeReleasedEvent);

            // Assert
            _themesCollectionAggregate.Collection.Should().ContainSingle(t => t.ThemeId == 2);
            var parent = _themesCollectionAggregate.Collection.Single(t => t.ThemeId == 1);
            parent.Children.Should().ContainSingle(t => t.ThemeId == 2);
        }

        [Fact]
        public void when_checking_if_theme_exists_and_it_does_not_exist_it_should_return_false()
        {
            // Arrange

            // Act
            var exists = _themesCollectionAggregate.Exists(1);

            // Assert
            exists.Should().BeFalse();
        }

        [Fact]
        public void when_checking_if_theme_exists_and_it_exists_it_should_return_true()
        {
            // Arrange
            var themeReleasedEvent = new ThemeReleased(1, null, "Test");
            _themesCollectionAggregate.Apply(themeReleasedEvent);

            // Act
            var exists = _themesCollectionAggregate.Exists(1);

            // Assert
            exists.Should().BeTrue();
        }

        [Fact]
        public void when_adding_theme_and_it_does_not_exist_it_should_add_event()
        {
            // Arrange
            var theme = new LegoThemesListAsyncResponse.Result(2, null, "Theme");

            // Act
            _themesCollectionAggregate.Add(theme);

            // Assert
            _themesCollectionAggregate.Events.Should().ContainSingle(e => e.Event is ThemeReleased);
        }

        [Fact]
        public void when_adding_theme_and_it_exists_it_should_not_add_event()
        {
            // Arrange
            var themeReleasedEvent = new ThemeReleased(1, null, "Test");
            _themesCollectionAggregate.Apply(themeReleasedEvent);
            var theme = new LegoThemesListAsyncResponse.Result(1, null, "Test");

            // Act
            _themesCollectionAggregate.Add(theme);

            // Assert
            _themesCollectionAggregate.Events.Should().BeEmpty();
        }

    }
}