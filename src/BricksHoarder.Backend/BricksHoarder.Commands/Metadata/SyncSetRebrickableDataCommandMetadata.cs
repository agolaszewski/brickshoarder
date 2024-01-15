namespace BricksHoarder.Commands.Metadata;

public static class SyncSetRebrickableDataCommandMetadata
{
    public const string Consumer = "SyncSetRebrickableDataCommandConsumer";
    public const string QueuePath = "SyncSetRebrickableDataCommand";
    public static readonly Uri QueuePathUri = new Uri($"queue:{QueuePath}");
}