using BricksHoarder.Core.Commands;
using BricksHoarder.Domain;
using BricksHoarder.Functions.Flows.Generator.Flows;
using MassTransit;
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
            "Azure.Messaging.ServiceBus",
            "Microsoft.Extensions.Logging"
        };

        private static readonly List<string> RequiredCommandsConsumedNamespaces = new()
        {
            "BricksHoarder.Events.Metadata",
            "MassTransit",
            "Microsoft.Azure.Functions.Worker",
            "Azure.Messaging.ServiceBus",
            "Microsoft.Extensions.Logging"
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

    internal class EventsGenerator
    {
       
        private static readonly List<string> RequiredEventsNamespaces = new()
        {
            "BricksHoarder.Events.Metadata",
            "MassTransit",
            "Microsoft.Azure.Functions.Worker",
            "Azure.Messaging.ServiceBus"
        };

        internal static void ScheduleCommand(Type @event, Type command)
        {
            CreateMetadata(@event);
            CreateScheduleCommandFunction(@event,command);
        }

        private static void CreateMetadata(Type @event)
        {
            var compiled = Templates.EventMetadataTemplate.Replace("{{event}}", @event.Name);
            File.WriteAllText($"{Catalogs.EventsMetadataCatalog}\\{@event.Name}Metadata.cs", compiled);

        }

        private static void CreateScheduleCommandFunction(Type @event, Type command)
        {
            var compiled = Templates.EventScheduleCommandFunctionTemplate
                .Replace("{{event}}", @event.Name)
                .Replace("{{command}}", command.Name);

            var namespaces = RequiredEventsNamespaces.ToList();
            namespaces.Add("BricksHoarder.Core.Events");
            namespaces.Add("BricksHoarder.Events");
            namespaces.Add(command.Namespace!);

            namespaces = namespaces.OrderBy(x => x, new NamespaceComparer()).Select(x => $"using {x};").ToList();
            compiled = compiled.Replace("{{namespaces}}", string.Join(Environment.NewLine, namespaces));

            File.WriteAllText($"{Catalogs.FunctionsCatalog}\\{@event.Name}Function.cs", compiled);
        }
    }
}
