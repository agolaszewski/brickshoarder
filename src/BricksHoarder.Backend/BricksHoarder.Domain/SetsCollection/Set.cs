namespace BricksHoarder.Domain.SetsCollection
{
    public class Set
    {
        public string SetId { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public Set(string setId, DateTime lastModifiedDate)
        {
            SetId = setId;
            LastModifiedDate = lastModifiedDate;
        }
    }
}