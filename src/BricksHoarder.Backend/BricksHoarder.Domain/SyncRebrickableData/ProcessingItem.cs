namespace BricksHoarder.Domain.SyncRebrickableData
{
    public class ProcessingItem
    {
        public ProcessingItem()
        {
        }

        public ProcessingItem(string id, ProcessingState state)
        {
            Id = id;
            State = state;
        }

        public string Id { get; set; }

        public ProcessingState State { get; set; }
    }
}