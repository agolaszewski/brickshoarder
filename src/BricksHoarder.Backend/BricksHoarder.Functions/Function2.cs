using BricksHoarder.Commands.Themes;
using BricksHoarder.Core.Commands;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BricksHoarder.Functions
{
    public class SyncThemesFunction
    {
        private ICommandDispatcher _commandDispatcher;

        public SyncThemesFunction(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        [FunctionName("SyncThemesFunction")]
        public async Task Run([TimerTrigger("0 */5 * * * *", RunOnStartup = true)] TimerInfo trigger, ILogger log)
        {
            await _commandDispatcher.DispatchAsync(new SyncThemesCommand());
        }
    }
}