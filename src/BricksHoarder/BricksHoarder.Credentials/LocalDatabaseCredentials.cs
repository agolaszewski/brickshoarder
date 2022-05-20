﻿using BricksHoarder.Core.Credentials;
using Microsoft.Extensions.Configuration;

namespace BricksHoarder.Credentials
{
    public sealed record LocalDatabaseCredentials : IConnectionString
    {
        private readonly string _catalog;

        private readonly string _url;

        public LocalDatabaseCredentials(IConfiguration configuration, string prefix)
        {
            _url = configuration[$"{prefix}:Url"];
            _catalog = configuration[$"{prefix}:Catalog"];
        }

        public string ConnectionString =>
            $"Data Source={_url};Initial Catalog={_catalog};Persist Security Info=False;Integrated Security=True;";
    }
}
