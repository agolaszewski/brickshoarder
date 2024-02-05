using BricksHoarder.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BricksHoarder.Functions
{
    public class Function
    {
        private readonly ILogger<Function> _logger;
        private readonly ICacheService _cacheService;

        public Function(ILogger<Function> logger, ICacheService cacheService)
        {
            _logger = logger;
            _cacheService = cacheService;
        }

        [Function("HttpFunction")]
        public IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {
            _cacheService.SetAsync("TEST", "TEST", TimeSpan.FromDays(1));
            return new OkObjectResult($"123Welcome to Azure Functions, {req.Query["name"]}!");
        }
    }
}
