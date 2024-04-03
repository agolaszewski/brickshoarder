using Azure.Messaging.ServiceBus;
using BricksHoarder.Core.Aggregates;
using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using MassTransit;
using BricksHoarder.Azure.ServiceBus;
using Microsoft.Extensions.Logging;

namespace BricksHoarder.Functions;

public abstract class BaseFunction(IMessageReceiver receiver, ILogger<BaseFunction> logger)
{
    protected const string Default = "default";
    protected const string ServiceBusConnectionString = "ServiceBusConnectionString";

    public async Task HandleCommandAsync<TCommand, TAggregateRoot>(ServiceBusReceivedMessage command, CancellationToken cancellationToken) where TCommand : class, ICommand where TAggregateRoot : class, IAggregateRoot
    {
        try
        {
            await receiver.HandleConsumer<CommandConsumer<TCommand, TAggregateRoot>>(typeof(TCommand).Name, command, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Error in Consumer {0}", typeof(TCommand).Name);
            throw;
        }
    }

    public async Task HandleEventAsync<TEvent>(ServiceBusReceivedMessage @event, CancellationToken cancellationToken) where TEvent : class, IEvent
    {
        try
        {
            await receiver.HandleConsumer<EventConsumer<TEvent>>(nameof(TEvent), @event, cancellationToken);
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
            await receiver.HandleSaga<TSaga>(topic, subscription, @event, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Error in Saga {0} {1}", typeof(TSaga).Name, topic);
            throw;
        }
    }

    public async Task ScheduleAsync<TCommand, TEvent>(ServiceBusReceivedMessage @event, string topic, string subscription, CancellationToken cancellationToken) where TCommand : class, ICommand where TEvent : class, IEvent, IScheduling<TCommand>    
    {
        try
        {
            await receiver.HandleConsumer<SchedulingConsumer<TCommand,TEvent>>(topic, subscription, @event, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Error in SchedulingConsumer {0} {1} {2}", typeof(TCommand).Name, typeof(TEvent).Name, topic);
            throw;
        }
    }
}