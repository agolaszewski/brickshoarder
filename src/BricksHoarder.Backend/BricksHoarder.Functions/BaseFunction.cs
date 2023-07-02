using Azure.Messaging.ServiceBus;
using BricksHoarder.AzureServiceBus;
using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using MassTransit;
using System;
using System.Threading;
using System.Threading.Tasks;

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

    public async Task HandleCommand<TCommand>(ServiceBusReceivedMessage command, CancellationToken cancellationToken) where TCommand : class, ICommand
    {
        try
        {
            await _receiver.HandleConsumer<CommandConsumer<TCommand>>(typeof(TCommand).Name, command, cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task HandleEvent<TEvent>(ServiceBusReceivedMessage @event, CancellationToken cancellationToken) where TEvent : class, IEvent
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

    public async Task HandleSaga<TSaga>(ServiceBusReceivedMessage @event, string topic, string subscription, CancellationToken cancellationToken) where TSaga : class, ISaga
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