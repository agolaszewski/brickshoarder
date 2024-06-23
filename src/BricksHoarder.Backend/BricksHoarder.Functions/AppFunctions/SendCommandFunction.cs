using System.Text.Json;
using BricksHoarder.Core.Commands;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;

namespace BricksHoarder.Functions.AppFunctions
{
    public class SendCommandFunction
    {
        private readonly ICommandDispatcher _commandDispatcher;

        public SendCommandFunction(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        [Function("SendCommand")]
        public async Task RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {
            var commandsAssembly = AppDomain.CurrentDomain.GetAssemblies().Single(assembly => assembly.GetName().Name == "BricksHoarder.Commands");

            string typeFullName = req.Headers["Command-Type"]!;
            Type type = commandsAssembly.GetType(typeFullName)!;

            object command = await JsonSerializer.DeserializeAsync(req.Body, type);
            await _commandDispatcher.DispatchAsync(command!);
        }
    }
}