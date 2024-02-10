using BricksHoarder.Core.Events;
using BricksHoarder.DateTime;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using System.Text.Json;

namespace BricksHoarder.Functions
{
    public class Test
    {
        private readonly IEventDispatcher _eventDispatcher;
        private readonly IDateTimeProvider _dataTimeProvider;

        public Test(IEventDispatcher eventDispatcher, IDateTimeProvider dataTimeProvider)
        {
            _eventDispatcher = eventDispatcher;
            _dataTimeProvider = dataTimeProvider;
        }

        [Function("SendEvent")]
        public async Task Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {
            var eventsAssembly = AppDomain.CurrentDomain.GetAssemblies().Single(assembly => assembly.GetName().Name == "BricksHoarder.Events");

            string typeFullName = req.Headers["Event-Type"]!;
            Type type = eventsAssembly.GetType(typeFullName)!;

            object @event = await JsonSerializer.DeserializeAsync(req.Body, type);
            await _eventDispatcher.DispatchAsync(@event);
        }
    }
}