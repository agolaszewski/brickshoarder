namespace BricksHoarder.Domain.Sets
{
    public class ProcessingItem
    {
        public ProcessingItem()
        {
        }

        public ProcessingItem(int id, ProcessingState state)
        {
            Id = id;
            State = state;
        }

        public int Id { get; set; }

        public ProcessingState State { get; set; }
    }
}