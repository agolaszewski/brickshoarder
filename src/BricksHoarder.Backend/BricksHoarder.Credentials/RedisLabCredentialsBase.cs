using BricksHoarder.Core.Credentials;
using BricksHoarder.Helpers;
using Microsoft.Extensions.Configuration;

namespace BricksHoarder.Credentials
{
    public sealed record RedisLabCredentialsBase : IConnectionString
    {
        public RedisLabCredentialsBase(IConfiguration configuration)
        {
            ConnectionString = configuration.Get("Redis:ConnectionString");
        }

        public string ConnectionString { get; }
    }
}