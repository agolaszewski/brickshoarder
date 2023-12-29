namespace BricksHoarder.Domain.ThemesCollection
{
    public record ThemesCollectionSnapshot
    {
        public ThemesCollectionSnapshot()
        {
        }

        public ThemesCollectionSnapshot(ThemesCollectionAggregate aggregate)
        {
            Themes = aggregate.Collection.Select(c => new ThemeSnapshot(c)).ToList();
            Version = aggregate.Version;
        }

        public List<ThemeSnapshot> Themes { get; set; }

        public long Version { get; set; }
    }
}