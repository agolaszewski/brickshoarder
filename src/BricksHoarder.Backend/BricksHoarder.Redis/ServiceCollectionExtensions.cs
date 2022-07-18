using BricksHoarder.Core.Services;
using BricksHoarder.Credentials;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace BricksHoarder.Redis
{
    public static class ServiceCollectionExtensions
    {
        public static void AddRedis(this IServiceCollection services, RedisCredentials redisCredentials)
        {
            services.AddSingleton(b => ConnectionMultiplexer.Connect(redisCredentials.ConnectionString));

            services.AddSingleton(b =>
            {
                var redis = b.GetRequiredService<ConnectionMultiplexer>();
                return redis.GetDatabase();
            });

            services.AddScoped<ICacheService, RedisCacheService>();
        }
    }
}
