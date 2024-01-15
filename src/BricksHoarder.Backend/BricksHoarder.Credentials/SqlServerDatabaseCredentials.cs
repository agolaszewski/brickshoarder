using BricksHoarder.Core.Credentials;
using BricksHoarder.Helpers;
using Microsoft.Extensions.Configuration;

namespace BricksHoarder.Credentials
{
    public sealed record SqlServerDatabaseCredentials : IConnectionString
    {
        private readonly string _catalog;

        private readonly string _password;

        private readonly string _url;

        private readonly string _user;

        public SqlServerDatabaseCredentials(IConfiguration configuration, string prefix)
        {
            _url = configuration.Get($"{prefix}:Url");
            _catalog = configuration.Get($"{prefix}:Catalog");
            _user = configuration.Get($"{prefix}:User");
            _password = configuration.Get($"{prefix}:Password");
        }

        public string ConnectionString =>
            $"Server={_url};Initial Catalog={_catalog};Persist Security Info=False;User ID={_user};Password={_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;";
    }
}