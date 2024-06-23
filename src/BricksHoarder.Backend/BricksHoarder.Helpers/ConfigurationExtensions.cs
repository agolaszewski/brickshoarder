using Microsoft.Extensions.Configuration;

namespace BricksHoarder.Helpers
{
    public static class ConfigurationExtensions
    {
        public static string Get(this IConfiguration configuration, string key)
        {
            string result = configuration[key];
            if (string.IsNullOrWhiteSpace(result))
            {
                throw new ArgumentNullException($"{key} is null");
            }

            return result;
        }
    }
}