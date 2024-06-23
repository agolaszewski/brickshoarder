using BricksHoarder.Helpers;
using Microsoft.Extensions.Configuration;

namespace BricksHoarder.Credentials
{
    public class RebrickableCredentials
    {
        public RebrickableCredentials(IConfiguration configuration)
        {
            Url = new Uri(configuration.Get("Rebrickable:Url"));
            Key = $"key {configuration.Get("Rebrickable:Key")}";
        }

        public string Key { get; }

        public Uri Url { get; }
    }
}