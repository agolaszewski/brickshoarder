using BricksHoarder.Common.CQRS;
using BricksHoarder.Core.Commands;
using BricksHoarder.Credentials;
using BricksHoarder.Jobs;
using MassTransit;
using MassTransit.ServiceBusIntegration;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BricksHoarder.RabbitMq
{
    public static class ServiceCollectionExtensions
    {
        public static void AddAzureServiceBus(this IServiceCollection services, AzureServiceBusCredentials credentials)
        {
            services.AddScoped<ICommandDispatcher, CommandDispatcher>();
            services.AddScoped<RequestToCommandMapper>();

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
                x.AddJobsConsumers();

                x.AddBus(context => Bus.Factory.CreateUsingAzureServiceBus(cfg =>
                {
                    //cfg.ConfigureJsonSerializerOptions(config =>
                    //{
                    //    config.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
                    //    return config;
                    //});
                    cfg.Host(credentials.ConnectionString, x =>
                    {
                    });

                    cfg.ReceiveEndpoint("commands", ec =>
                    {
                        foreach (var commandType in commands)
                        {
                            var typeArguments = commandType.GetGenericArguments();
                            ec.ConfigureConsumer(context, typeof(CommandConsumer<>).MakeGenericType(typeArguments));
                        }
                        ec.UseJobsConsumers(context);
                    });
                }));
            });
        }

        public static void AddAzureServiceBusForAzureFunction(this IServiceCollection services, AzureServiceBusCredentials credentials)
        {
            services.AddSingleton<IMessageReceiver, MessageReceiver>();
            services.AddSingleton<IAsyncBusHandle, AsyncBusHandle>();

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

                x.UsingAzureServiceBus((context, cfg) =>
                {
                    var options = context.GetRequiredService<IOptions<ServiceBusOptions>>();
                    options.Value.AutoCompleteMessages = true;

                    cfg.Host(credentials.ConnectionString, x =>
                    {
                    });

                    cfg.UseServiceBusMessageScheduler();
                });
            });

            services.RemoveMassTransitHostedService();
        }
    }
}