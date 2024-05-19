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
        public void AddNonExistingThemeToCollection()
        {
            // Arrange

            var theme = new LegoThemesListAsyncResponse.Result(1, null, "Test Theme");

            // Act
            _themesCollectionAggregate.Add(theme);

            // Assert
            _themesCollectionAggregate.Collection.Any(x => x.ThemeId == 1).Should().BeTrue();
        }

        [Fact]
        public void AddExistingThemeToCollection()
        {
            // Test to verify that adding an existing theme to the collection does not add it again

            // Arrange
            var theme = new LegoThemesListAsyncResponse.Result(1, null, "Test Theme");

            // Act
            _themesCollectionAggregate.Add(theme);
            _themesCollectionAggregate.Add(theme);

            // Assert
            _themesCollectionAggregate.Collection.Count(x => x.ThemeId == 1).Should().Be(1);
        }

        [Fact]
        public void AddThemeWithParentToCollection()
        {
            // Test to verify adding a theme with a parent to the collection

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