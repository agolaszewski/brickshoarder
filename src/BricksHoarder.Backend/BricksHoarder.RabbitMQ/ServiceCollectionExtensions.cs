using BricksHoarder.Common.CQRS;
using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using BricksHoarder.Credentials;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace BricksHoarder.RabbitMq
{
    public static class ServiceCollectionExtensions
    {
        public static void AddRabbitMq(this IServiceCollection services, RabbitMqCredentials credentials)
        {
            services.AddScoped<ICommandDispatcher, CommandDispatcher>();
            services.AddScoped<RequestToCommandMapper>();
            services.AddScoped<IIntegrationEventsQueue, IntegrationEventsQueue>();

            services.AddMassTransit(x =>
            {
                var domainAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .SingleOrDefault(assembly => assembly.GetName().Name == "BricksHoarder.Domain");

                var commands = domainAssembly.GetTypes()
                    .Where(t => t.IsNested && t.Name == "Handler")
                    .Select(t => t.GetInterfaces().First())
                    .Where(t => typeof(ICommandHandler<>).IsAssignableFrom(t.GetGenericTypeDefinition()))
                    .ToList();

                foreach (var commandType in commands)
                {
                    var typeArguments = commandType.GetGenericArguments();
                    x.AddConsumer(typeof(CommandConsumer<>).MakeGenericType(typeArguments));
                }

                var sagas = domainAssembly.GetTypes()
                    .Where(t => t.Name.EndsWith("Saga"));

                foreach (var sagaType in sagas)
                {
                    x.AddSagaStateMachine(sagaType);
                }
                x.SetInMemorySagaRepositoryProvider();

                x.AddBus(context => Bus.Factory.CreateUsingRabbitMq(cfg =>
                {
                    cfg.PurgeOnStartup = true;
                    //cfg.ConfigureJsonSerializerOptions(config =>
                    //{
                    //    config.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
                    //    return config;
                    //});

                    cfg.Host(credentials.Url, h =>
                    {
                        h.Username(credentials.UserName);
                        h.Password(credentials.Password);
                    });
                    cfg.UseInMemoryScheduler();

                    //cfg.ReceiveEndpoint("commands", ec =>
                    //{
                    //    foreach (var commandType in commands)
                    //    {
                    //        var typeArguments = commandType.GetGenericArguments();
                    //        ec.ConfigureConsumer(context, typeof(CommandConsumer<>).MakeGenericType(typeArguments));
                    //    }
                    //});
                }));
            });
        }
    }
}