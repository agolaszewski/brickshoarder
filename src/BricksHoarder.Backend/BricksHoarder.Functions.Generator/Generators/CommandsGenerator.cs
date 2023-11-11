using BricksHoarder.Commands;
using BricksHoarder.Domain;
using ICommand = BricksHoarder.Core.Commands.ICommand;

namespace BricksHoarder.Functions.Generator.Generators
{
    internal class CommandsGenerator : BaseGenerator
    {
        private readonly IReadOnlyList<Type> _sagas;
        private readonly List<Type> _commands;

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

            var commandsAssembly = typeof(BricksHoarderCommandsAssemblyPointer).Assembly.GetTypes();
            _commands = commandsAssembly.Where(t => t.GetInterface(nameof(ICommand)) is not null).ToList();
        }

        public void GenerateMetadata()
        {
            foreach (var command in _commands)
            {
                var compiled = Templates.CommandMetadataTemplate.Replace("{{command}}", command.Name);
                File.WriteAllText($"{Catalogs.CommandMetadataCatalog}\\{command.Name}Metadata.cs", compiled);

                compiled = Templates.CommandConsumedMetadataTemplate.Replace("{{command}}", command.Name);
                File.WriteAllText($"{Catalogs.EventsMetadataCatalog}\\{command.Name}ConsumedMetadata.cs", compiled);
            }
        }

        public void GenerateFunctions()
        {
            foreach (var command in _commands)
            {
                CreateFunctionForSaga(command);
                CreateFunction(command);
            }
        }

        private void CreateFunction(Type command)
        {
            var compiled = Templates.CommandFunctionTemplate.Replace("{{command}}", command.Name);

            var namespaces = _requiredCommandsNamespaces.ToList();
            namespaces.Add(command.Namespace!);
            namespaces = namespaces.OrderBy(x => x, new NamespaceComparer()).Select(x => $"using {x};").ToList();

            compiled = compiled.Replace("{{namespaces}}", string.Join(Environment.NewLine, namespaces));
            File.WriteAllText($"{Catalogs.FunctionsCatalog}\\{command.Name}Function.cs", compiled);
        }

        private void CreateFunctionForSaga(Type command)
        {
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