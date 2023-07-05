namespace BricksHoarder.MsSql.Database.Snapshots.Queries
{
    public class SqlScripts
    {
        public SqlScripts()
        {
            SnapshotInsert = File.ReadAllText("SnapshotInsert.sql");
            SnapshotGet = File.ReadAllText("SnapshotGet.sql");
        }

        public string SnapshotInsert { get; }
        public string SnapshotGet { get; }
    }
}