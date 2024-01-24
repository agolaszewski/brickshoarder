using BricksHoarder.Core.Credentials;
using BricksHoarder.Helpers;
using Microsoft.Extensions.Configuration;

namespace BricksHoarder.Credentials
{
    public sealed record RedisAzureCredentialsBase : IConnectionString
    {
        private readonly bool _allowAdmin;

        private readonly bool _isSsl;

        private readonly string _name;

        private readonly string _password;

        private readonly int _port;

        public RedisAzureCredentialsBase(IConfiguration configuration)
        {
            _name = configuration.Get("Redis:Name");
            _password = configuration.Get("Redis:Password");
            _port = configuration.Get("Redis:Port").To<int>();

            _isSsl = true;
            _allowAdmin = true;
        }

        public string ConnectionString =>
            $"{_name}.redis.cache.windows.net:{_port},password={_password},ssl={_isSsl},abortConnect=False,allowAdmin={_allowAdmin}";
    }

    public sealed record RedisLabCredentialsBase : IConnectionString
    {
        public RedisLabCredentialsBase(IConfiguration configuration)
        {
            ConnectionString = configuration.Get("Redis:ConnectionString");
        }

        public string ConnectionString { get; }
    }
}