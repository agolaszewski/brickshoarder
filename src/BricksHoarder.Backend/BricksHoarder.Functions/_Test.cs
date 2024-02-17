using BricksHoarder.Core.Events;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using System.Text.Json;

namespace BricksHoarder.Functions
{
    public class _Test
    {
        private readonly IEventDispatcher _eventDispatcher;

        public _Test(IEventDispatcher eventDispatcher)
        {
            _eventDispatcher = eventDispatcher;
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