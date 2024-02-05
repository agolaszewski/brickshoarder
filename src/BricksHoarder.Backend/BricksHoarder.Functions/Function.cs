using BricksHoarder.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Configuration;

namespace BricksHoarder.Functions
{
    public class Function
    {
        private readonly ILogger<Function> _logger;
        private readonly ICacheService _cacheService;
        private readonly IConfiguration _configuration;

        public Function(ILogger<Function> logger, ICacheService cacheService, IConfiguration configuration)
        {
            _logger = logger;
            _cacheService = cacheService;
            _configuration = configuration;
        }

        [Function("HttpFunction")]
        public IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {
            _logger.LogWarning(_configuration["Redis:ConnectionString"]);
            _cacheService.SetAsync("TEST", "TEST", TimeSpan.FromDays(1));
            return new OkObjectResult($"123Welcome to Azure Functions, {req.Query["name"]}!");
        }
    }
}
