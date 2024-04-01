using BricksHoarder.Commands.Sets;
using BricksHoarder.Commands.Themes;
using BricksHoarder.Domain.SyncRebrickableData;
using BricksHoarder.Events;
using BricksHoarder.Functions.Flows.Generator.Flows;

namespace BricksHoarder.Functions.Flows.Generator
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var flow = new Flow();
            var saga = flow.Saga<SyncRebrickableDataSagaState>();
            saga.Event<SyncSagaStarted>();
            saga.Event<CommandConsumed<SyncThemesCommand>>();
            saga.Event<CommandConsumed<SyncSetsCommand>>();
            saga.Event<BatchEvent<SetReleased>>();
            saga.Event<SetDetailsChanged>();
            saga.Event<CommandConsumed<SyncSetRebrickableDataCommand>>();
            saga.Event<NoChangesToSets>();

            saga.Command<SyncThemesCommand>();
            saga.Command<SyncSetsCommand>();
            saga.Command<SyncSetLegoDataCommand>();
            saga.Command<SyncSetRebrickableDataCommand>();

            var syncSetLegoDataCommand = flow.Command<SyncSetLegoDataCommand>();
            syncSetLegoDataCommand.Event<LegoSetInSale>().Command<SyncSetLegoDataCommand>(nameof(LegoSetInSale.NextJobRun));
            syncSetLegoDataCommand.Event<LegoSetToBeReleased>().Command<SyncSetLegoDataCommand>(nameof(LegoSetToBeReleased.ReleaseDate));
            syncSetLegoDataCommand.Event<LegoSetPending>().Command<SyncSetLegoDataCommand>(nameof(LegoSetPending.PendingUntil));

            flow.Build();
        }
    }
}