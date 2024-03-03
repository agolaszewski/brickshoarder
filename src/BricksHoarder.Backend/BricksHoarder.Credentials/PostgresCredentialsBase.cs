using BricksHoarder.Core.Credentials;

namespace BricksHoarder.Credentials
{
    public class PostgresCredentialsBase : IConnectionString
    {
        public PostgresCredentialsBase()
        {
            
        }

        public string ConnectionString { get; }
    }
}