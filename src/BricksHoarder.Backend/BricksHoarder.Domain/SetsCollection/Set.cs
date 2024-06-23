namespace BricksHoarder.Domain.SetsCollection
{
    public class Set
    {
        public string SetId { get; set; }

        public System.DateTime LastModifiedDate { get; set; }

        public Set(string setId, System.DateTime lastModifiedDate)
        {
            SetId = setId;
            LastModifiedDate = lastModifiedDate;
        }
    }
}