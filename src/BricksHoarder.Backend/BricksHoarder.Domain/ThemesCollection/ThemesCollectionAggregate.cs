using BricksHoarder.Common.DDD.Aggregates;
using BricksHoarder.Core.Aggregates;
using BricksHoarder.Events;
using MessagePack;
using RebrickableApi;

namespace BricksHoarder.Domain.ThemesCollection
{
    [MessagePackObject(true)]
    public class ThemesCollectionAggregate : AggregateRoot<ThemesCollectionAggregate>,
        IApply<ThemeReleased>
    {

        private readonly List<Theme> _collection = new();

        public IReadOnlyList<Theme> Collection => _collection;

        public ThemesCollectionAggregate()
        {
        }

        public ThemesCollectionAggregate(ThemesCollectionSnapshot snapshot)
        {
            foreach (var theme in snapshot.Themes)
            {
                var parent = _collection.FirstOrDefault(item => item.Id == theme.ParentId);

                var newTheme = new Theme(theme.Id, theme.Name, parent);
                parent?.Children.Add(newTheme);

                _collection.Add(newTheme);
            }

            Version = snapshot.Version;
        }

        public void Apply(ThemeReleased @event)
        {
            var parent = _collection.FirstOrDefault(item => item.Id == @event.ParentId);

            var newTheme = new Theme(@event.Id, @event.Name, parent);
            parent?.Children.Add(newTheme);

            _collection.Add(newTheme);
        }

        public bool Exists(int id) => _collection.Any(item => item.Id == id);

        public void Add(LegoThemesListAsyncResponse.Result theme)
        {
            if (Exists(theme.Id))
            {
                return;
            }

            AddEvent(new ThemeReleased(theme.Id, theme.ParentId, theme.Name));
        }
    }
}