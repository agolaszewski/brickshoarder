namespace BricksHoarder.MsSql.Database.Queries
{
    public class SqlScripts
    {
        public SqlScripts()
        {
            CacheClean = File.ReadAllText("Queries\\CleanCache\\CleanCache.sql");
        }

        public string CacheClean { get; }
    }
}