using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FunctionApp4
{
    public class Function2
    {
        private readonly ILogger _logger;

        public Function2(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Function2>();
        }

        [Function("Function2")]
        public async Task Run([TimerTrigger("0 */5 * * * *", RunOnStartup = true)] TimerInfo myTimer)
        {
            try
            {
                _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            }
            catch (Exception a)
            {
                _logger.LogCritical(a.Message);
                throw;
            }
        }
    }
}