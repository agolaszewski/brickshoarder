using BricksHoarder.Core.Credentials;

namespace BricksHoarder.Credentials
{
    public record EventStoreCredentials : IConnectionString
    {
        public EventStoreCredentials(EventStoreCredentialsBase credentials)
        {
            ConnectionString = credentials.ConnectionString;
        }

        public string ConnectionString { get; }
    }
}