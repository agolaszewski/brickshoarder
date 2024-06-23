using BricksHoarder.Core.Events;
using BricksHoarder.DateTime.Noda;
using BricksHoarder.Events;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;

namespace BricksHoarder.Functions.Timers
{
    //public class Function(IEventDispatcher eventDispatcher, IDateTimeProvider dataTimeProvider)
    //{
    //    [Function("Function")]
    //    public async Task Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
    //    {
    //        await eventDispatcher.DispatchAsync(new SyncSagaStarted(dataTimeProvider.UtcNow().Date.ToGuid()));
    //    }
    //}
}