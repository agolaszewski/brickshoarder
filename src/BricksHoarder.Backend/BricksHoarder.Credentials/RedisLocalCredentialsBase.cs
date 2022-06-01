using BricksHoarder.Core.Credentials;
using Microsoft.Extensions.Configuration;

namespace BricksHoarder.Credentials
{
    public sealed record RedisLocalCredentialsBase : IConnectionString
    {
        private readonly bool _allowAdmin;

        public RedisLocalCredentialsBase(IConfiguration configuration)
        {
            _allowAdmin = true;
        }

        public string ConnectionString => $"localhost,abortConnect=False,allowAdmin={_allowAdmin}";
    }
}
