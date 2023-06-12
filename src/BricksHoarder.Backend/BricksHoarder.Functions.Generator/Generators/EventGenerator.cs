using BricksHoarder.Commands;
using BricksHoarder.Core.Commands;
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
        private readonly IReadOnlyList<Type> _commands;

        public EventGenerator()
        {
            var eventsAssembly = typeof(BricksHoarderEventsAssemblyPointer).Assembly.GetTypes();
            _events = eventsAssembly.Where(t => t.GetInterface(nameof(IEvent)) is not null).Where(t => !t.IsGenericType).ToList();

            var domainAssembly = typeof(BricksHoarderDomainAssemblyPointer).Assembly.GetTypes();
            _sagas = domainAssembly.Where(IsSaga).ToList();

            var commandsAssembly = typeof(BricksHoarderCommandsAssemblyPointer).Assembly.GetTypes();
            _commands = commandsAssembly.Where(t => t.GetInterface(nameof(ICommand)) is not null).ToList();
        }

        private bool IsSaga(Type type)
        {
            return type.Name.EndsWith("Saga");
        }

        private bool IsEventUsedBySaga(Type saga, Type @event)
        {
            var properties = saga.GetProperties();
            return properties.Any(p => IsEventInSaga(p.PropertyType, @event));
        }

        private bool IsEventInSaga(Type property, Type @event)
        {
            if (!property.IsGenericType)
            {
                return false;
            }

            var @args = property.GetGenericArguments();
            return @args.Any(a => a == @event);
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

    internal abstract class BaseGenerator
    {
    }

    public static class Catalogs
    {
        public const string EventsMetadataCatalog = "BricksHoarder.Events\\Metadata";
        public const string FunctionsCatalog = "BricksHoarder.Functions";
    }

    public static class Templates
    {
        private const string TemplateCatalogPath = "BricksHoarder.Functions.Generator\\Templates";
        public static string EventMetadataTemplate = File.ReadAllText($"{TemplateCatalogPath}\\EventMetadata.tmpl");
        public static string EventFunctionTemplate = File.ReadAllText($"{TemplateCatalogPath}\\EventFunction.tmpl");
    }
}