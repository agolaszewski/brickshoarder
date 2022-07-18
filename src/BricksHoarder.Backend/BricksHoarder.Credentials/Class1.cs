using BricksHoarder.Core.Credentials;
using BricksHoarder.Helpers;
using Microsoft.Extensions.Configuration;

namespace BricksHoarder.Credentials
{
    public class AzureServiceBusCredentials : IConnectionString
    {
        private string _endpoint;
        private string _sharedAccessKeyName;
        private string _sharedAccessKey;

        public AzureServiceBusCredentials(IConfiguration configuration, string prefix)
        {
            _endpoint = configuration.Get($"{prefix}:Endpoint");
            _sharedAccessKeyName = configuration.Get($"{prefix}:SharedAccessKeyName");
            _sharedAccessKey = configuration.Get($"{prefix}:SharedAccessKey");
        }

        public string ConnectionString => $"Endpoint=sb://{_endpoint};SharedAccessKeyName={_sharedAccessKeyName};SharedAccessKey={_sharedAccessKey}";
    }
}