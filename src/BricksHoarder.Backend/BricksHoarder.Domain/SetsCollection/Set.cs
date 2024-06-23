namespace BricksHoarder.Domain.SetsCollection
{
    public class Set(string setId, System.DateTime lastModifiedDate)
    {
        public string SetId { get; set; } = setId;
        public System.DateTime LastModifiedDate { get; set; } = lastModifiedDate;
    }
}