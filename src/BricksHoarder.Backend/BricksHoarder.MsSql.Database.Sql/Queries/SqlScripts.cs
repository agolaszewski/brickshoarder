namespace BricksHoarder.MsSql.Database.Queries
{
    public class SqlScripts
    {
        public SqlScripts()
        {
            CacheInsert = File.ReadAllText("Queries\\CacheInsert\\CacheInsert.sql");
            CacheGet = File.ReadAllText("Queries\\CacheGet\\CacheGet.sql");
            CacheClean = File.ReadAllText("Queries\\CacheClean\\CacheClean.sql");
        }

        public string CacheInsert { get; }

        public string CacheGet { get; }

        public string CacheClean { get; }
    }
}