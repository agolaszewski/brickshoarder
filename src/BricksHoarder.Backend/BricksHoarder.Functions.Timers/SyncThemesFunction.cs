using BricksHoarder.Core.Events;
using BricksHoarder.DateTime.Noda;
using BricksHoarder.Events;
using Microsoft.Azure.Functions.Worker;

namespace BricksHoarder.Functions.Timers
{
    public class SyncThemesFunction(IEventDispatcher eventDispatcher, IDateTimeProvider dataTimeProvider)
    {
        [Function("SyncSagaFunction")]
        public async Task RunAsync([TimerTrigger("0 0 3 * * *")] TimerInfo trigger)
        {
            await eventDispatcher.DispatchAsync(new SyncSagaStarted(dataTimeProvider.UtcNow().Date.ToGuid()));
        }
    }
}