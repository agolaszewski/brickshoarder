using BricksHoarder.Core.Events;
using BricksHoarder.Domain;
using BricksHoarder.Events;
using MassTransit;

namespace BricksHoarder.Functions.Generator.Generators
{
    internal class EventGenerator : BaseGenerator
    {
        private readonly IReadOnlyList<Type> _events;
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
        }

        public void GenerateFunctionsForSagas()
        {
            foreach (var @event in _events)
            {
                var eventType = typeof(Event<>);
                var consumed = typeof(CommandConsumed<>);
                var genericConsumed = consumed.MakeGenericType(@eventType);
                var genericEventType = eventType.MakeGenericType(genericConsumed);

                var saga = _sagas.FirstOrDefault(s => IsUsedBySaga(s, genericEventType));
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