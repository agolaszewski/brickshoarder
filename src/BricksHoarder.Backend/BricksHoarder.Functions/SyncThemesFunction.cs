using BricksHoarder.Core.Events;
using BricksHoarder.DateTime;
using BricksHoarder.Events;
using Microsoft.Azure.WebJobs;
using System.Threading.Tasks;

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

        [FunctionName("SyncSagaFunction")]
        public async Task Run([TimerTrigger("0 */5 * * * *", RunOnStartup = true)] TimerInfo trigger)
        {
            await _eventDispatcher.DispatchAsync(new SyncSagaStarted(_dataTimeProvider.UtcNow().ToString("yyyyMMdd")));
        }
    }
}