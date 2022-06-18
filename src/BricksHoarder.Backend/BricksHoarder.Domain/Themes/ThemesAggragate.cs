using BricksHoarder.Common.DDD.Aggregates;
using BricksHoarder.Core.Aggregates;
using BricksHoarder.Core.Events;

namespace BricksHoarder.Domain.Themes
{
    public class ThemesCollectionAggregate : AggregateRoot<ThemesCollectionAggregate>,
        IApply<ThemeCreated>
    {
        private readonly List<Theme> _collection = new();

        public void Apply(ThemeCreated @event)
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

        public Theme? Get(int id) => _collection.FirstOrDefault(item => item.Id == id);

        public bool IsMinifigureTheme(int id)
        {
            return false;
        }
    }

    public class Theme
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public Theme? Parent { get; set; }

        public List<Theme> Children { get; set; } = new();
    }

    public class ThemeCreated : IEvent
    {
        public int Id { get; set; }

        public int? ParentId { get; set; }

        public string Name { get; set; }

        public Guid CorrelationId { get; set; }
    }
}