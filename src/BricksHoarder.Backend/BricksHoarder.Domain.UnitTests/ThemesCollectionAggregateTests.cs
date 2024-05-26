using BricksHoarder.Domain.ThemesCollection;
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
    }
}