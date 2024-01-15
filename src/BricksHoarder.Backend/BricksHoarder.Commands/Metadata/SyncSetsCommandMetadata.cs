namespace BricksHoarder.Commands.Metadata;

public static class SyncSetsCommandMetadata
{
    public const string Consumer = "SyncSetsCommandConsumer";
    public const string QueuePath = "SyncSetsCommand";
    public static readonly Uri QueuePathUri = new Uri($"queue:{QueuePath}");
}