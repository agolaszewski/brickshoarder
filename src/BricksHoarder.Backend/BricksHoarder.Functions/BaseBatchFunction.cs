using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using BricksHoarder.Core.Events;
using BricksHoarder.Events;

namespace BricksHoarder.Functions
{
    public abstract class BaseBatchFunction
    {
        private readonly IEventDispatcher _eventDispatcher;
        protected const string Default = "default";
        protected const string ServiceBusConnectionString = "ServiceBusConnectionString";

        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        protected BaseBatchFunction(IEventDispatcher eventDispatcher)
        {
            _eventDispatcher = eventDispatcher;
        }

        public async Task HandleBatchAsync<TMessage>(ServiceBusReceivedMessage[] @events) where TMessage : class, IBatch
        {
            try
            {
                var groups = @events.GroupBy(e => e.CorrelationId).Select(group => new
                {
                    CorrelationId = Guid.Parse(group.Key),
                    Collection = group.ToList()
                }).ToList();

                var batches = groups.Select(group =>
                {
                    var messages = new List<TMessage>();

                    foreach (var message in group.Collection)
                    {
                        var body = Encoding.UTF8.GetString(message.Body);

                        using var jsonParse = JsonDocument.Parse(body);
                        var element = jsonParse.RootElement.GetProperty("message");
                        var msg = element.Deserialize<TMessage>(_options);

                        messages.Add(msg);
                    }

                    return new BatchEvent<TMessage>(group.CorrelationId, messages);
                
                }).ToList();

                foreach (var batch in batches)
                {
                    await _eventDispatcher.DispatchAsync(batch, batch.CorrelationId);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}