using BricksHoarder.Core.Events;
using BricksHoarder.Domain;
using BricksHoarder.Events;
using MassTransit;
using System;
using static MassTransit.Logging.OperationName;

namespace BricksHoarder.Functions.Generator.Generators
{
    internal class EventGenerator : BaseGenerator
    {
        private readonly IReadOnlyList<Type> _events;
        private readonly IReadOnlyList<Type> _batchEvent;
        private readonly IReadOnlyList<Type> _sagas;

        private readonly List<string> _requiredEventsNamespaces = new()
        {
            "BricksHoarder.Events.Metadata",
            "MassTransit",
            "Microsoft.Azure.Functions.Worker"
        };

        public EventGenerator()
        {
            var eventsAssembly = typeof(BricksHoarderEventsAssemblyPointer).Assembly.GetTypes();
            _events = eventsAssembly.Where(t => t.GetInterface(nameof(IEvent)) is not null).Where(t => !t.IsGenericType).ToList();
            _batchEvent = eventsAssembly.Where(t => t.GetInterface(nameof(IBatch)) is not null).Where(t => !t.IsGenericType).ToList();

            var domainAssembly = typeof(BricksHoarderDomainAssemblyPointer).Assembly.GetTypes();
            _sagas = domainAssembly.Where(IsSaga).ToList();
        }

        public void GenerateMetadata()
        {
            foreach (var @event in _events)
            {
                var compiled = Templates.EventMetadataTemplate.Replace("{{event}}", @event.Name);
                File.WriteAllText($"{Catalogs.EventsMetadataCatalog}\\{@event.Name}Metadata.cs", compiled);
            }

            foreach (var @event in _batchEvent)
            {
                var compiled = Templates.EventBatchMetadataTemplate.Replace("{{event}}", @event.Name);
                File.WriteAllText($"{Catalogs.EventsMetadataCatalog}\\{@event.Name}BatchMetadata.cs", compiled);
            }
        }

        public void GenerateFunctionsForSagas()
        {
            var events = _events.Where(t => t.GetInterface(nameof(IBatch)) is null).ToList();

            foreach (var @event in events)
            {
                var eventType = typeof(Event<>);
                var eventTypeGeneric = eventType.MakeGenericType(@event);
                
                var saga = _sagas.FirstOrDefault(s => IsUsedBySaga(s, eventTypeGeneric));
                if (saga is null)
                {
                    continue;
                }

                var eventHandler = $"await HandleSagaAsync<{saga.Name}State>(@event, {@event.Name}Metadata.TopicPath, Default, cancellationToken);";

                var compiled = Templates.EventFunctionTemplate.Replace("{{event}}", @event.Name);
                compiled = compiled.Replace("{{eventHandler}}", eventHandler);

                var namespaces = _requiredEventsNamespaces.ToList();
                namespaces.Add(saga.Namespace!);
                namespaces = namespaces.OrderBy(x => x, new NamespaceComparer()).Select(x => $"using {x};").ToList();

                compiled = compiled.Replace("{{namespaces}}", string.Join(Environment.NewLine, namespaces));

                File.WriteAllText($"{Catalogs.FunctionsCatalog}\\{@event.Name}Function.cs", compiled);
            }
        }

        public void GenerateBatchFunctionsForSagas()
        {
            foreach (var @event in _batchEvent)
            {
                var batchEvent = typeof(BatchEvent<>);
                var genericBatch = batchEvent.MakeGenericType(@event);

                var eventType = typeof(Event<>);
                var genericEventType = eventType.MakeGenericType(genericBatch);

                var saga = _sagas.FirstOrDefault(s => IsUsedBySaga(s, genericEventType));
                if (saga is null)
                {
                    continue;
                }

                BatchFunction(@event);
                BatchConsumerFunction(saga,@event);
            }
        }

        private void BatchFunction(Type @event)
        {
            var compiled = Templates.EventBatchFunctionTemplate.Replace("{{event}}", @event.Name);

            var namespaces = _requiredEventsNamespaces.ToList();
            namespaces.Add("BricksHoarder.Core.Events");
            namespaces.Add("BricksHoarder.Events");

            namespaces = namespaces.OrderBy(x => x, new NamespaceComparer()).Select(x => $"using {x};").ToList();
            compiled = compiled.Replace("{{namespaces}}", string.Join(Environment.NewLine, namespaces));

            File.WriteAllText($"{Catalogs.FunctionsCatalog}\\{@event.Name}Function.cs", compiled);
        }

        private void BatchConsumerFunction(Type saga, Type @event)
        {
            var eventHandler = $"await HandleSagaAsync<{saga.Name}State>(@event, {@event.Name}BatchMetadata.TopicPath, Default, cancellationToken);";

            var compiled = Templates.EventFunctionTemplate.Replace("{{event}}", @event.Name + "Batch");
            compiled = compiled.Replace("{{eventHandler}}", eventHandler);

            var namespaces = _requiredEventsNamespaces.ToList();
            namespaces.Add(saga.Namespace!);
            namespaces = namespaces.OrderBy(x => x, new NamespaceComparer()).Select(x => $"using {x};").ToList();

            compiled = compiled.Replace("{{namespaces}}", string.Join(Environment.NewLine, namespaces));

            File.WriteAllText($"{Catalogs.FunctionsCatalog}\\{@event.Name}BatchFunction.cs", compiled);
        }

        public void Initialize()
        {
            var commandsMetadata = Directory.GetFiles(Catalogs.CommandMetadataCatalog, "*.cs");
            foreach (var file in commandsMetadata)
            {
                File.Delete(file);
            }

            var eventsMetadata = Directory.GetFiles(Catalogs.EventsMetadataCatalog, "*.cs");
            foreach (var file in eventsMetadata)
            {
                File.Delete(file);
            }

            var files = Directory.GetFiles(Catalogs.FunctionsCatalog, "*.cs");
            foreach (var file in files)
            {
                var text = File.ReadAllText(file);
                if (text.Contains(": BaseFunction"))
                {
                    File.Delete(file);
                }
            }
        }
    }
}