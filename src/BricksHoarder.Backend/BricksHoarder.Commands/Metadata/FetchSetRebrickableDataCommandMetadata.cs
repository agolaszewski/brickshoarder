namespace BricksHoarder.Commands.Metadata;

public static class FetchSetRebrickableDataCommandMetadata
{
    public const string Consumer = "FetchSetRebrickableDataCommandConsumer";
    public const string QueuePath = "FetchSetRebrickableDataCommand";
    public static readonly Uri QueuePathUri = new Uri($"queue:{QueuePath}");
}