using BricksHoarder.Commands;
using BricksHoarder.Core.Commands;
using BricksHoarder.Domain;

namespace BricksHoarder.Functions.Generator.Generators
{
    internal class CommandsGenerator : BaseGenerator
    {
        public record CommandHandlerType(Type Command, Type Aggregate);

        private readonly IReadOnlyList<Type> _sagas;
        private readonly List<CommandHandlerType> _commandHandlers;

        private readonly List<string> _requiredCommandsNamespaces = new()
        {
            "BricksHoarder.Commands.Metadata",
            "MassTransit",
            "Microsoft.Azure.Functions.Worker"
        };

        private readonly List<string> _requiredCommandsConsumedNamespaces = new()
        {
            "BricksHoarder.Events.Metadata",
            "MassTransit",
            "Microsoft.Azure.Functions.Worker"
        };

        public CommandsGenerator()
        {
            var domainAssembly = typeof(BricksHoarderDomainAssemblyPointer).Assembly.GetTypes();
            _sagas = domainAssembly.Where(IsSaga).ToList();

            _commandHandlers = domainAssembly
                .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>)))
                .Select(h =>
                {
                    var commandHandler = h.GetInterfaces().First();
                    var args = commandHandler.GetGenericArguments();
                    return new CommandHandlerType(args[0], args[1]);
                }).ToList();
        }

        public void GenerateMetadata()
        {
            foreach (var handler in _commandHandlers)
            {
                var command = handler.Command;
                var compiled = Templates.CommandMetadataTemplate.Replace("{{command}}", command.Name);
                File.WriteAllText($"{Catalogs.CommandMetadataCatalog}\\{command.Name}Metadata.cs", compiled);

                compiled = Templates.CommandConsumedMetadataTemplate.Replace("{{command}}", command.Name);
                File.WriteAllText($"{Catalogs.EventsMetadataCatalog}\\{command.Name}ConsumedMetadata.cs", compiled);
            }
        }

        public void GenerateFunctions()
        {
            foreach (var handler in _commandHandlers)
            {
                CreateFunctionForSaga(handler);
                CreateFunction(handler);
            }
        }

        private void CreateFunction(CommandHandlerType handler)
        {
            var command = handler.Command;
            var aggregate = handler.Aggregate;

            var compiled = Templates.CommandFunctionTemplate.Replace("{{command}}", command.Name);
            compiled = compiled.Replace("{{aggregate}}", handler.Aggregate.Name);

            var namespaces = _requiredCommandsNamespaces.ToList();
            namespaces.Add(command.Namespace!);
            namespaces.Add(aggregate.Namespace!);
            namespaces = namespaces.OrderBy(x => x, new NamespaceComparer()).Select(x => $"using {x};").ToList();

            compiled = compiled.Replace("{{namespaces}}", string.Join(Environment.NewLine, namespaces));

            File.WriteAllText($"{Catalogs.FunctionsCatalog}\\{command.Name}Function.cs", compiled);
        }

        private void CreateFunctionForSaga(CommandHandlerType handler)
        {
            var command = handler.Command;

            var saga = _sagas.FirstOrDefault(s => IsEventUsedBySaga(s, command));
            if (saga is null)
            {
                return;
            }

            var eventHandler = $"await HandleSaga<{saga.Name}State>(@event, {command.Name}ConsumedMetadata.TopicPath, Default, cancellationToken);";

            var compiled = Templates.EventFunctionTemplate.Replace("{{event}}", $"{command.Name}Consumed");
            compiled = compiled.Replace("{{eventHandler}}", eventHandler);

            var namespaces = _requiredCommandsConsumedNamespaces.ToList();
            namespaces.Add(saga.Namespace!);
            namespaces = namespaces.OrderBy(x => x, new NamespaceComparer()).Select(x => $"using {x};").ToList();

            compiled = compiled.Replace("{{namespaces}}", string.Join(Environment.NewLine, namespaces));

            File.WriteAllText($"{Catalogs.FunctionsCatalog}\\{command.Name}ConsumedFunction.cs", compiled);
        }
    }
}