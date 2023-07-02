public static class SyncThemesCommandMetadata
{
    public const string Consumer = "SyncThemesCommandConsumer";
    public const string QueuePath = "SyncThemesCommand";
    public static readonly Uri QueuePathUri = new Uri($"queue:{QueuePath}");
}