using BricksHoarder.Azure.ServiceBus.Services;
using BricksHoarder.Common.CQRS;
using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using BricksHoarder.Credentials;
using MassTransit;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BricksHoarder.Azure.ServiceBus
{
    public static class ServiceCollectionExtensions
    {
        public static void AddAzureServiceBus(this IServiceCollection services, AzureServiceBusCredentials credentials, Action<IBusRegistrationConfigurator> busRegistrationConfigurator, Action<IBusRegistrationContext, IServiceBusBusFactoryConfigurator> busConfiguration)
        {
            services.AddScoped<RequestToCommandMapper>();
            services.AddScoped<ICommandDispatcher, CommandDispatcher>();
            services.AddScoped<IEventDispatcher, EventDispatcher>();
            services.AddScoped<IIntegrationEventsQueue, IntegrationEventsQueue>();
           
            services.AddAzureClients(builder =>
            {
                builder.AddServiceBusClient(credentials.ConnectionString).WithName("ServiceBusClient");
                builder.AddServiceBusAdministrationClient(credentials.ConnectionString)
                    .WithName("ServiceBusAdministrationClient");
            });
            services.AddScoped<DeadLetterQueueRescheduler>();
            services.AddScoped<ResubmitDeadQueueService>();

            services.AddMassTransit(x =>
            {
                busRegistrationConfigurator(x);

                x.AddServiceBusMessageScheduler();

                x.UsingAzureServiceBus((context, cfg) =>
                {
                    var options = context.GetRequiredService<IOptions<ServiceBusOptions>>();
                    cfg.Host(credentials.ConnectionString, _ => { });

                    cfg.UseServiceBusMessageScheduler();

                    busConfiguration(context, cfg);

                    cfg.Publish<IEvent>(x => x.Exclude = true);
                    cfg.Publish<ICommand>(x => x.Exclude = true);
                    
                    cfg.ConfigureEndpoints(context);
                });
            });
        }

        public static void AddAzureServiceBusForAzureFunction(this IServiceCollection services, AzureServiceBusCredentials credentials, Action<IBusRegistrationConfigurator> busRegistrationConfigurator, Action<IBusRegistrationContext, IServiceBusBusFactoryConfigurator> busConfiguration)
        {
            services.AddScoped<IEventDispatcher, EventDispatcher>();

            services.AddMassTransitForAzureFunctions(x =>
            {
                busRegistrationConfigurator(x);
                x.AddServiceBusMessageScheduler();
                
            }, connectionStringConfigurationKey:credentials.ConnectionString, (context, cfg) =>
            {
                busConfiguration(context, cfg);

                cfg.Publish<IEvent>(x => x.Exclude = true);
                cfg.Publish<ICommand>(x => x.Exclude = true);

                cfg.ConfigureEndpoints(context);
            });
        }
    }
}