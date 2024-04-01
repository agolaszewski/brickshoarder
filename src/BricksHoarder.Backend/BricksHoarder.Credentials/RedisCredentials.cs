using BricksHoarder.Core.Credentials;

namespace BricksHoarder.Credentials
{
    public record RedisCredentials : IConnectionString
    {
        public RedisCredentials(RedisLocalCredentialsBase credentials)
        {
            ConnectionString = credentials.ConnectionString;
        }

        public RedisCredentials(RedisAzureCredentialsBase credentials)
        {
            ConnectionString = credentials.ConnectionString;
        }

        public RedisCredentials(RedisLabCredentialsBase credentials)
        {
            ConnectionString = credentials.ConnectionString;
        }

        public string ConnectionString { get; }
    }
}