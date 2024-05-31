﻿using BricksHoarder.Azure.ServiceBus.Services;
using BricksHoarder.Commands.Sets;
using BricksHoarder.Commands.Themes;
using BricksHoarder.Common.CQRS;
using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using BricksHoarder.Credentials;
using BricksHoarder.Domain.SyncRebrickableData;
using BricksHoarder.Events;
using BricksHoarder.Events.Metadata;
using MassTransit;
using MassTransit.AzureServiceBusTransport;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BricksHoarder.Azure.ServiceBus
{
    public static class ServiceCollectionExtensions
    {
        public static void AddAzureServiceBus2(this IServiceCollection services, AzureServiceBusCredentials credentials, Action<IBusRegistrationConfigurator> busRegistrationConfigurator, Action<IBusRegistrationContext, IServiceBusBusFactoryConfigurator> busConfiguration)
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

                //x.AddConsumer(typeof(SchedulingConsumer<SyncSetLegoDataCommand, LegoSetInSale>));
                //x.AddConsumer(typeof(SchedulingConsumer<SyncSetLegoDataCommand, LegoSetToBeReleased>));
                //x.AddConsumer(typeof(SchedulingConsumer<SyncSetLegoDataCommand, LegoSetPending>));

                x.AddServiceBusMessageScheduler();

                x.UsingAzureServiceBus((context, cfg) =>
                {
                    var options = context.GetRequiredService<IOptions<ServiceBusOptions>>();
                    cfg.Host(credentials.ConnectionString, _ => { });

                    busConfiguration(context, cfg);

                    cfg.Publish<IEvent>(x => x.Exclude = true);
                    cfg.Publish<ICommand>(x => x.Exclude = true);

                    cfg.ConfigureEndpoints(context);
                });
            });
        }
    }
}