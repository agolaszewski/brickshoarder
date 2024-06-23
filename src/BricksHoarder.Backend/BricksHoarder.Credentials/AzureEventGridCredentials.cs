using BricksHoarder.Helpers;
using Microsoft.Extensions.Configuration;

namespace BricksHoarder.Credentials
{
    public class AzureEventGridCredentials
    {
        public string TopicEndpoint { get; }

        public string TopicAccessKey { get; }

        public AzureEventGridCredentials(IConfiguration configuration, string topic)
        {
            TopicEndpoint = configuration.Get($"EventGrid:{topic}:TopicEndpoint");
            TopicAccessKey = configuration.Get($"EventGrid:{topic}:TopicAccessKey");
        }
    }
}