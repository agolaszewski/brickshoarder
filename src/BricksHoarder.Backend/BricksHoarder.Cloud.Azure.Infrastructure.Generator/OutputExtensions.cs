using Pulumi;

namespace BricksHoarder.Cloud.Azure.Infrastructure.Generator;

public static class OutputExtensions
{
    public static T Convert<T>(this Output<T> @that)
    {
        var value = default(T);
        @that.Apply(x =>
        {
            value = x;
            return x;
        });
        return value!;
    }
}