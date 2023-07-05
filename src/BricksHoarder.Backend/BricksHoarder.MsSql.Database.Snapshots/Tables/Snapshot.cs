namespace BricksHoarder.MsSql.Database.Snapshots.Tables
{
    public class Snapshot
    {
        public Snapshot()
        {
            
        }

        public Snapshot(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; set; }

        public string Value { get; set; }
    }
}