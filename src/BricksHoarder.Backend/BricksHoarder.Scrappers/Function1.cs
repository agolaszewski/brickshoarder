using System;
using System.Text.Json;
using BricksHoarder.Websites.Scrappers.Lego;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BricksHoarder.Scrappers
{
    public class Function1
    {
        private readonly ILogger _logger;
        private readonly LegoScrapper _lego;

        public Function1(LegoScrapper lego, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Function1>();
            _lego = lego;
        }

        [Function("Lego")]
        public async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer)
        {
            _logger.LogWarning($"TEST");
        }
    }
}
