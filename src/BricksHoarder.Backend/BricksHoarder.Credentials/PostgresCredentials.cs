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

    public class PostgresAzureCredentials : IConnectionString
    {
        private readonly string _host;
        private readonly string _database;
        private readonly string _password;
        private readonly string _username;

        public PostgresAzureCredentials(IConfiguration configuration, string prefix)
        {
            _host = configuration.Get($"{prefix}:Host");
            _database = configuration.Get($"{prefix}:Database");
            _password = configuration.Get($"{prefix}:Password");
            _username = configuration.Get($"{prefix}:Username");
        }

        public string ConnectionString => $"host={_host};port=5432;database={_database};username={_username};password={_password};SSL Mode=Require;Trust Server Certificate=true";
    }
}