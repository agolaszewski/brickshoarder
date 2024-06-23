namespace BricksHoarder.Commands.Metadata;

public static class SyncSetLegoDataCommandMetadata
{
    public const string Consumer = "SyncSetLegoDataCommandConsumer";
    public const string QueuePath = "SyncSetLegoDataCommand";
    public static readonly Uri QueuePathUri = new Uri($"queue:{QueuePath}");
}