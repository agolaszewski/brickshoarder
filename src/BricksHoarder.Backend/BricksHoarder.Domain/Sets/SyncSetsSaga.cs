using BricksHoarder.Commands.Themes;
using BricksHoarder.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BricksHoarder.Domain.Sets
{
    public class SyncSetsState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }

        public int CurrentState { get; set; }
    }

    public class SyncSetsSaga : MassTransitStateMachine<SyncSetsState>
    {
        public SyncSetsSaga(ILogger<SyncSetsSaga> logger)
        {
            InstanceState(x => x.CurrentState, ProcessingState);

            Event(() => ThemesSynced, x => { x.CorrelateById(context => context.CorrelationId!.Value); });
            Event(() => SyncSagaStarted, x => { x.CorrelateById(context => context.CorrelationId!.Value); });

            Initially(When(SyncSagaStarted)
                .TransitionTo(ProcessingState)
                .Then(_ => logger.LogInformation("XD"))
                .Publish(new SyncThemesCommand()));
                //.ThenAsync(async x =>
                //{
                //    var endpoint = x.ReceiveContext.SendEndpointProvider;
                //    var send = await endpoint.GetSendEndpoint(new Uri("queue:commands"));
                //    await send.Send(new SyncThemesCommand(), xx => xx.CorrelationId = x.CorrelationId);
                //}));

            Initially(When(ThemesSynced)
                .Then(_ => logger.LogInformation("XD"))
                .Finalize());
        }

        public State ProcessingState { get; }

        public Event<SyncSagaStarted> SyncSagaStarted { get; }
        public Event<ThemesSynced> ThemesSynced { get; }
    }
}