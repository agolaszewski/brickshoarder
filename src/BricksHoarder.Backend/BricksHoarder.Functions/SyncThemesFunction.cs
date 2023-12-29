using BricksHoarder.Core.Events;
using BricksHoarder.DateTime;
using BricksHoarder.Events;
using Microsoft.Azure.Functions.Worker;

namespace BricksHoarder.Functions
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
        public async Task RunAsync([TimerTrigger("0 */1 * * * *", RunOnStartup = true)] TimerInfo trigger)
        {
            try
            {
                await _eventDispatcher.DispatchAsync(new SyncSagaStarted(_dataTimeProvider.UtcNow().ToString("yyyyMMdd")));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}