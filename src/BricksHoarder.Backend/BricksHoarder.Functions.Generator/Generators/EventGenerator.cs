using BricksHoarder.Core.Events;
using BricksHoarder.Domain;
using BricksHoarder.Events;

namespace BricksHoarder.Functions.Generator.Generators
{
    internal class EventGenerator : BaseGenerator
    {
        private readonly IReadOnlyList<Type> _events;
        private readonly IReadOnlyList<Type> _sagas;

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
                var saga = _sagas.FirstOrDefault(s => IsEventUsedBySaga(s, @event));
                if (saga is null)
                {
                    continue;
                }

                var eventHandler = $"await HandleSaga<{saga.Name}State>(@event, {@event.Name}Metadata.TopicPath, Default, cancellationToken);";

                var compiled = Templates.EventFunctionTemplate.Replace("{{event}}", @event.Name);
                compiled = compiled.Replace("{{eventHandler}}", eventHandler);
                compiled = compiled.Replace("{{sagaNamespace}}", saga.Namespace);

                File.WriteAllText($"{Catalogs.FunctionsCatalog}\\{@event.Name}Function.cs", compiled);
            }
        }
    }
}