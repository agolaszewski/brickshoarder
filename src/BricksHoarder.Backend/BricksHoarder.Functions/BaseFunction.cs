using Azure.Messaging.ServiceBus;
using BricksHoarder.AzureCloud.ServiceBus;
using BricksHoarder.Core.Aggregates;
using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using BricksHoarder.Events;
using MassTransit;
using System.Text;
using System.Text.Json;

namespace BricksHoarder.Functions;

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

public abstract class BaseFunction
{
    protected const string Default = "default";
    protected const string ServiceBusConnectionString = "ServiceBusConnectionString";
    private readonly IMessageReceiver _receiver;

    protected BaseFunction(IMessageReceiver receiver)
    {
        _receiver = receiver;
    }

    public async Task HandleCommandAsync<TCommand, TAggregateRoot>(ServiceBusReceivedMessage command, CancellationToken cancellationToken) where TCommand : class, ICommand where TAggregateRoot : class, IAggregateRoot
    {
        try
        {
            await _receiver.HandleConsumer<CommandConsumer<TCommand, TAggregateRoot>>(typeof(TCommand).Name, command, cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task HandleEventAsync<TEvent>(ServiceBusReceivedMessage @event, CancellationToken cancellationToken) where TEvent : class, IEvent
    {
        try
        {
            await _receiver.HandleConsumer<EventConsumer<TEvent>>(nameof(TEvent), @event, cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task HandleSagaAsync<TSaga>(ServiceBusReceivedMessage @event, string topic, string subscription, CancellationToken cancellationToken) where TSaga : class, ISaga
    {
        try
        {
            await _receiver.HandleSaga<TSaga>(topic, subscription, @event, cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}