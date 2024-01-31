using BricksHoarder.Core.Credentials;
using BricksHoarder.Helpers;
using Microsoft.Extensions.Configuration;

namespace BricksHoarder.Credentials
{
    public class PostgresCredentials : IConnectionString
    {
        private readonly string _host;
        private readonly string _database;
        private readonly string _password;
        private readonly string _username;

        public PostgresCredentials(IConfiguration configuration, string prefix)
        {
            _host = configuration.Get($"{prefix}:Host");
            _database = configuration.Get($"{prefix}:Database");
            _password = configuration.Get($"{prefix}:Password");
            _username = configuration.Get($"{prefix}:Username");
        }

        public string ConnectionString => $"host={_host};database={_database};password={_password};username={_username}";
    }

    public class PostgresCredentialsBase : IConnectionString
    {
        public PostgresCredentialsBase()
        {
            
        }

        public string ConnectionString { get; }
    }
}