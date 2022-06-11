namespace BricksHoarder.Jobs
{
    public record SyncSetsJobInput
    {
        public int PageNumber { get; set; }
    }
}