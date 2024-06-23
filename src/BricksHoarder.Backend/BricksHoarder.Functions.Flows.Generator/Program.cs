using BricksHoarder.Commands.Sets;
using BricksHoarder.Commands.Themes;
using BricksHoarder.Domain.SyncRebrickableData;
using BricksHoarder.Events;
using BricksHoarder.Functions.Flows.Generator.Flows;
using System.Diagnostics;
using MassTransit;

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
            saga.Event<SetReleased>().AsBatch();
            saga.Event<SetDetailsChanged>().AsBatch();
            saga.Event<CommandConsumed<SyncSetRebrickableDataCommand>>();
            saga.Event<NoChangesToSets>();

            saga.Command<SyncThemesCommand>();
            saga.Command<SyncSetsCommand>();
            var syncSetLegoDataCommand = saga.Command<SyncSetLegoDataCommand>();
            saga.Command<SyncSetRebrickableDataCommand>();

            //syncSetLegoDataCommand.Event<NewLegoSetDiscovered>().Command<DownloadLegoSetImage>();
            syncSetLegoDataCommand.Event<LegoSetInSale>().Schedule<SyncSetLegoDataCommand>();
            syncSetLegoDataCommand.Event<LegoSetToBeReleased>().Schedule<SyncSetLegoDataCommand>();
            syncSetLegoDataCommand.Event<LegoSetPending>().Schedule<SyncSetLegoDataCommand>();

            flow.Build();
        }
    }
}