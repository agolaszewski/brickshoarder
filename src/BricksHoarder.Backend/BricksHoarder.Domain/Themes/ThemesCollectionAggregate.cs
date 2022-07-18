using BricksHoarder.Common.DDD.Aggregates;
using BricksHoarder.Core.Aggregates;
using BricksHoarder.Events;
using RebrickableApi;

namespace BricksHoarder.Domain.Themes
{
    public class ThemesCollectionAggregate : AggregateRoot<ThemesCollectionAggregate>,
        IApply<ThemeAdded>
    {
        private readonly List<Theme> _collection = new();

        public void Apply(ThemeAdded @event)
        {
            var parent = _collection.FirstOrDefault(item => item.Id == @event.ParentId);
            var newTheme = new Theme()
            {
                Id = @event.Id,
                Parent = parent,
                Name = @event.Name
            };
            parent?.Children.Add(newTheme);

            _collection.Add(newTheme);
        }

        public bool Exists(int id) => _collection.Any(item => item.Id == id);

        public bool IsMiniFigureTheme(LegoThemesListAsyncResponse.Result theme)
        {
            return theme.Id == 535 || theme?.ParentId == 535;
        }

        public void Add(LegoThemesListAsyncResponse.Result theme)
        {
            if (Exists(theme.Id) || IsMiniFigureTheme(theme))
            {
                return;
            }

            AddEvent(new ThemeAdded(theme.Id, theme.ParentId, theme.Name));
        }
    }
}