using BricksHoarder.Helpers;
using Microsoft.Extensions.Configuration;

namespace BricksHoarder.Credentials
{
    public class RebrickableCredentials
    {
        public RebrickableCredentials(IConfiguration configuration)
        {
            Url = configuration.Get("Rebrickable:Url");
            Key = $"key {configuration.Get("Rebrickable:Key")}";
        }

        public string Key { get; }

        public string Url { get; }
    }
}