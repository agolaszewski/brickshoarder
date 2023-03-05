//using BricksHoarder.Commands.Themes;
//using BricksHoarder.Core.Commands;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using BricksHoarder.Commands.Themes;
using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using BricksHoarder.Events;

namespace BricksHoarder.Functions
{
    public class SyncThemesFunction
    {
        private readonly IEventDispatcher _eventDispatcher;

        public SyncThemesFunction(IEventDispatcher eventDispatcher)
        {
            _eventDispatcher = eventDispatcher;
        }

        [FunctionName("SyncSagaFunction")]
        public async Task Run([TimerTrigger("0 */1 * * * *", RunOnStartup = true)] TimerInfo trigger)
        {
            await _eventDispatcher.DispatchAsync(new SyncSagaStarted());
        }
    }
}