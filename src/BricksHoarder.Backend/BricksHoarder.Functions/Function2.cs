using System;
using System.Threading.Tasks;
using BricksHoarder.Commands.Themes;
using BricksHoarder.Core.Commands;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace BricksHoarder.Functions
{
    public class JobsTrigger
    {
        private ICommandDispatcher _commandDispatcher;

        public JobsTrigger(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        [FunctionName("JobsTrigger")]
        public async Task Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, ILogger log)
        {
            await _commandDispatcher.DispatchAsync(new SyncThemesCommand()
            {
                CorrelationId = Guid.Empty
            });
        }
    }
}
