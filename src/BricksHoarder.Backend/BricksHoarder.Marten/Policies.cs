using Polly;

namespace BricksHoarder.Marten
{
    internal static class Policies
    {
        internal static readonly AsyncPolicy SqRetryPolicy = Policy.Handle<Exception>().WaitAndRetryAsync(new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2) });
        internal static readonly AsyncPolicy EventStoreRetryPolicy = Policy.Handle<Exception>().WaitAndRetryAsync(new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2) });

        //internal static readonly AsyncPolicy<bool> RedisFallbackPolicy = Policy<bool>.Handle<Exception>().FallbackAsync(x => Task.FromResult(true));
        //internal static readonly AsyncPolicy<RedisValue> RedisValueFallbackPolicy = Policy<RedisValue>.Handle<Exception>().FallbackAsync(x => Task.FromResult(RedisValue.Null));
        internal static readonly AsyncPolicy PublishRetryPolicy = Policy.Handle<Exception>().WaitAndRetryAsync(new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2) });
    }
}