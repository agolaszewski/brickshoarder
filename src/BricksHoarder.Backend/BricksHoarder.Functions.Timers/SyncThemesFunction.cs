using BricksHoarder.Core.Events;
using BricksHoarder.DateTime.Noda;
using BricksHoarder.Events;
using Microsoft.Azure.Functions.Worker;

namespace BricksHoarder.Functions.Timers
{
    public class SyncThemesFunction
    {
        private readonly IEventDispatcher _eventDispatcher;
        private readonly IDateTimeProvider _dataTimeProvider;

        public SyncThemesFunction(IEventDispatcher eventDispatcher, IDateTimeProvider dataTimeProvider)
        {
            _eventDispatcher = eventDispatcher;
            _dataTimeProvider = dataTimeProvider;
        }

        [Function("SyncSagaFunction")]
        public async Task RunAsync([TimerTrigger("0 0 3 * * *")] TimerInfo trigger)
        {
            await _eventDispatcher.DispatchAsync(new SyncSagaStarted(_dataTimeProvider.UtcNow().Date.ToGuid()));
        }
    }
}