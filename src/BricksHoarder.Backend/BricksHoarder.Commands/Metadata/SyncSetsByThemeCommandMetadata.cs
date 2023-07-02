public static class SyncSetsByThemeCommandMetadata
{
    public const string Consumer = "SyncSetsByThemeCommandConsumer";
    public const string QueuePath = "SyncSetsByThemeCommand";
    public static readonly Uri QueuePathUri = new Uri($"queue:{QueuePath}");
}