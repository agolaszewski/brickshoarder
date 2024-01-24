using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BricksHoarder.Functions
{
    public class Function
    {
        private readonly ILogger _logger;

        public Function(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Function>();
        }

        [Function("Function")]
        public void Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer)
        {
            _logger.LogWarning($"C# Timer trigger function executed at: {System.DateTime.Now}");
            
            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogWarning($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            }
        }
    }
}
