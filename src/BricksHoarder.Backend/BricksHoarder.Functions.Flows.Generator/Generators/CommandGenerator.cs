using BricksHoarder.Core.Commands;
using BricksHoarder.Domain;
using BricksHoarder.Functions.Flows.Generator.Flows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BricksHoarder.Functions.Flows.Generator.Generators
{
    internal class CommandGenerator
    {
        public record CommandHandlerType(Type Command, Type Aggregate);

        private static readonly List<string> RequiredCommandsNamespaces = new()
        {
            "BricksHoarder.Commands.Metadata",
            "MassTransit",
            "Microsoft.Azure.Functions.Worker",
            "Azure.Messaging.ServiceBus"
        };

        private static readonly List<string> RequiredCommandsConsumedNamespaces = new()
        {
            "BricksHoarder.Events.Metadata",
            "MassTransit",
            "Microsoft.Azure.Functions.Worker",
            "Azure.Messaging.ServiceBus"
        };

        private static readonly Type[] DomainAssembly;

        static CommandGenerator()
        {
            DomainAssembly = typeof(BricksHoarderDomainAssemblyPointer).Assembly.GetTypes();
        }

        internal static void Generate(Type command)
        {
        
            var commandHandlerType = DomainAssembly
                .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>)))
                .Select(t => t.GetInterfaces().First())
                .Where(commandHandler =>
                {
                    var args = commandHandler.GetGenericArguments();
                    return args[0].FullName == command.FullName;
                })
                .Select(h =>
                {
                    var args = h.GetGenericArguments();
                    return new CommandHandlerType(args[0], args[1]);
                })
                .First();

            CreateMetadata(commandHandlerType);
            CreateFunction(commandHandlerType);
        }

        private static void CreateMetadata(CommandHandlerType handler)
        {
            var command = handler.Command;

            var compiled = Templates.CommandMetadataTemplate.Replace("{{command}}", command.Name);
            File.WriteAllText($"{Catalogs.CommandMetadataCatalog}\\{command.Name}Metadata.cs", compiled);

            compiled = Templates.CommandConsumedMetadataTemplate.Replace("{{command}}", command.Name);
            File.WriteAllText($"{Catalogs.EventsMetadataCatalog}\\{command.Name}ConsumedMetadata.cs", compiled);
        }

        private static void CreateFunction(CommandHandlerType handler)
        {
            var command = handler.Command;
            var aggregate = handler.Aggregate;

            var compiled = Templates.CommandFunctionTemplate.Replace("{{command}}", command.Name);
            compiled = compiled.Replace("{{aggregate}}", handler.Aggregate.Name);

            var namespaces = RequiredCommandsNamespaces.ToList();
            namespaces.Add(command.Namespace!);
            namespaces.Add(aggregate.Namespace!);
            namespaces = namespaces.OrderBy(x => x, new NamespaceComparer()).Select(x => $"using {x};").ToList();

            compiled = compiled.Replace("{{namespaces}}", string.Join(Environment.NewLine, namespaces));

            File.WriteAllText($"{Catalogs.FunctionsCatalog}\\{command.Name}Function.cs", compiled);
        }
    }
}
