using BricksHoarder.Commands.Sets;
using BricksHoarder.Commands.Themes;
using BricksHoarder.Domain.SyncRebrickableData;
using BricksHoarder.Events;
using BricksHoarder.Functions.Flows.Generator.Flows;
using System.Diagnostics;
//dotnet run --project ./BricksHoarder.Functions.Flows.Generator/BricksHoarder.Functions.Flows.Generator.csproj

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
            syncSetLegoDataCommand.Event<LegoSetInSale>().Schedule<SyncSetLegoDataCommand>();
            syncSetLegoDataCommand.Event<LegoSetToBeReleased>().Schedule<SyncSetLegoDataCommand>();
            syncSetLegoDataCommand.Event<LegoSetPending>().Schedule<SyncSetLegoDataCommand>();

            flow.Build();
        }
    }
}