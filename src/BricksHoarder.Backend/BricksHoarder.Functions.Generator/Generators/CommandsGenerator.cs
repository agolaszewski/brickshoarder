﻿using BricksHoarder.Commands;
using BricksHoarder.Core.Commands;
using BricksHoarder.Domain;
using BricksHoarder.Events;

namespace BricksHoarder.Functions.Generator.Generators
{
    internal class CommandsGenerator : BaseGenerator
    {
        private readonly IReadOnlyList<Type> _sagas;
        private readonly List<Type> _commands;
        private readonly Type _commandConsumedGenericType = typeof(CommandConsumed<>);

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
            compiled = compiled.Replace("{{commandNamespace}}", command.Namespace);
            File.WriteAllText($"{Catalogs.FunctionsCatalog}\\{command.Name}Function.cs", compiled);
        }

        private void CreateFunctionForSaga(Type command)
        {
            var @event = _commandConsumedGenericType.MakeGenericType(new Type[] { command });
            var saga = _sagas.FirstOrDefault(s => IsEventUsedBySaga(s, @event));
            if (saga is null)
            {
                return;
            }

            var eventHandler = $"await HandleSaga<{saga.Name}State>(@event, {@event.Name}Metadata.TopicPath, Default, cancellationToken);";

            var compiled = Templates.EventFunctionTemplate.Replace("{{event}}", @event.Name);
            compiled = compiled.Replace("{{eventHandler}}", eventHandler);
            compiled = compiled.Replace("{{sagaNamespace}}", saga.Namespace);

            File.WriteAllText($"{Catalogs.FunctionsCatalog}\\{@event.Name}Function.cs", compiled);
        }
    }
}