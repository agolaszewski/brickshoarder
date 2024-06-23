using BricksHoarder.Core.Credentials;
using BricksHoarder.Helpers;
using Microsoft.Extensions.Configuration;

namespace BricksHoarder.Credentials
{
    public sealed record EventStoreCredentialsBase : IConnectionString
    {
        public EventStoreCredentialsBase(IConfiguration configuration)
        {
            User = configuration.Get("EventStore:User");
            Password = configuration.Get("EventStore:Password");
            Ip = configuration.Get("EventStore:Ip");
            Port = configuration.Get("EventStore:Port").To<int>();
        }

        public string Ip { get; }

        public string Password { get; }

        public int Port { get; }

        public string User { get; }

        public string ConnectionString => $"tcp://{User}:{Password}@{Ip}:{Port}";
    }
}