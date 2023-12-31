using Azure.Messaging.ServiceBus;
using BricksHoarder.AzureCloud.ServiceBus;
using BricksHoarder.Core.Aggregates;
using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using MassTransit;

namespace BricksHoarder.Functions;

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