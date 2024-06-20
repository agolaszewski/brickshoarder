using Azure.Identity;
using BricksHoarder.Azure.ServiceBus;
using BricksHoarder.Commands.Sets;
using BricksHoarder.Commands.Themes;
using BricksHoarder.Common;
using BricksHoarder.Common.DDD.Exceptions;
using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using BricksHoarder.Core.Services;
using BricksHoarder.Credentials;
using BricksHoarder.DateTime.Noda;
using BricksHoarder.Domain;
using BricksHoarder.Domain.LegoSet;
using BricksHoarder.Domain.RebrickableSet;
using BricksHoarder.Domain.SetsCollection;
using BricksHoarder.Domain.SyncRebrickableData;
using BricksHoarder.Domain.ThemesCollection;
using BricksHoarder.Events;
using BricksHoarder.Marten;
using BricksHoarder.Playwright;
using BricksHoarder.Rebrickable;
using BricksHoarder.Redis;
using BricksHoarder.Serilog;
using BricksHoarder.Websites.Scrappers;
using BricksHoarder.Websites.Scrappers.Lego;
using Marten;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Azure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
//var builder = WebApplication.CreateBuilder(new WebApplicationOptions
//{
//    ContentRootPath = AppDomain.CurrentDomain.BaseDirectory,
//});

if (builder.Environment.IsDevelopment())
{
    Development(builder.Services, builder.Configuration);
}
else
{
    Production(builder.Services, builder.Configuration);
}

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

void Production(IServiceCollection services, IConfiguration config)
{
    Common(services, config);

    //services.AddApplicationInsightsTelemetryWorkerService();
    //services.ConfigureFunctionsApplicationInsights();

    var martenCredentials = new PostgresAzureCredentials(config, "MartenAzure");
    services.AddMartenEventStore(martenCredentials);
}

void Development(IServiceCollection services, IConfiguration config)
{
    Common(services, config);

    Log.Logger = Log.Logger
        .AddSerilog()
        .AddConsole()
        .AddSeq(new Uri("http://localhost:5341/")).CreateLogger();

    services.AddLogging(lb => lb.AddSerilog(Log.Logger, true));

    var martenCredentials = new PostgresCredentials(config, "MartenAzure");
    services.AddMartenEventStore(martenCredentials);
}

void Common(IServiceCollection services, IConfiguration config)
{
    services.AddDomain();
    services.AddAutoMapper(mapper =>
    {
        mapper.AddDomainProfiles();
    });

    services.CommonServices();
    services.AddDateTimeProvider();

    services.AddRebrickable(new RebrickableCredentials(config));

    var redisCredentials = new RedisCredentials(new RedisLabCredentialsBase(config));
    services.AddRedis(redisCredentials);
    services.AddMessageLockService();

    services.AddScrappers();
    services.AddPlaywright();

    services.AddAzureServiceBus(new AzureServiceBusCredentials(config, "AzureServiceBus"), bus =>
    {
        bus.AddCommandConsumer<SyncThemesCommand, ThemesCollectionAggregate>();
        bus.AddCommandConsumer<SyncSetsCommand, SetsCollectionAggregate>();
        bus.AddCommandConsumer<SyncSetRebrickableDataCommand, RebrickableSetAggregate>();
        bus.AddCommandConsumer<SyncSetLegoDataCommand, LegoSetAggregate>(cfg =>
        {
            cfg.ConcurrentMessageLimit = 5;

            cfg.UseDelayedRedelivery(r =>
            {
                r.Handle<DomainException>();
                r.Intervals(TimeSpan.FromHours(1), TimeSpan.FromHours(1), TimeSpan.FromDays(1), TimeSpan.FromDays(1), TimeSpan.FromDays(1), TimeSpan.FromDays(1), TimeSpan.FromDays(1));
            });
        });

        bus.AddConsumerSaga<SyncRebrickableDataSaga, SyncRebrickableDataSagaState>(redisCredentials);

        bus.AddConsumerBatch<SetReleased>(msg => msg.CorrelationId);
        bus.AddConsumerBatch<SetDetailsChanged>(msg => msg.CorrelationId);

        bus.AddScheduleCommandConsumer<SyncSetLegoDataCommand, LegoSetInSale>();
        bus.AddScheduleCommandConsumer<SyncSetLegoDataCommand, LegoSetToBeReleased>();
        bus.AddScheduleCommandConsumer<SyncSetLegoDataCommand, LegoSetPending>();

        bus.AddEventConsumer<CommandConsumedSyncSetLegoDataCommand, CommandConsumed<SyncSetLegoDataCommand>>();
    },
    (context, cfg) =>
    {
        cfg.ConfigureCommandConsumer<SyncThemesCommand, ThemesCollectionAggregate>(context);
        cfg.ConfigureCommandConsumer<SyncSetsCommand, SetsCollectionAggregate>(context);
        cfg.ConfigureCommandConsumer<SyncSetRebrickableDataCommand, RebrickableSetAggregate>(context);
        cfg.ConfigureCommandConsumer<SyncSetLegoDataCommand, LegoSetAggregate>(context);

        cfg.BatchSubscriptionEndpoint<SetReleased>(context);
        cfg.BatchSubscriptionEndpoint<SetDetailsChanged>(context);

        cfg.ScheduleSubscriptionEndpoint<SyncSetLegoDataCommand, LegoSetInSale>(context);
        cfg.ScheduleSubscriptionEndpoint<SyncSetLegoDataCommand, LegoSetToBeReleased>(context);
        cfg.ScheduleSubscriptionEndpoint<SyncSetLegoDataCommand, LegoSetPending>(context);

        cfg.CommandConsumedSubscriptionEndpoint<CommandConsumedSyncSetLegoDataCommand, SyncSetLegoDataCommand>(context, "lock");
    });
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/lego/{id}", async ([FromServices] LegoScrapper legoScrapper, ICommandDispatcher dispatcher, string id) =>
    {
        await dispatcher.DispatchAsync<SyncSetLegoDataCommand>(new SyncSetLegoDataCommand(id));
    })
.WithName("Lego")
.WithOpenApi();

app.MapGet("/saga/sync", async ([FromServices] IEventDispatcher dispatcher, ICacheService cache, IDocumentStore ds) =>
    {
        await cache.ClearAsync();
        await ds.Advanced.ResetAllData();

        await dispatcher.DispatchAsync<SyncSagaStarted>(new SyncSagaStarted(DateTime.UtcNow.Date.ToGuid()));
    })
.WithName("SyncSaga")
.WithOpenApi();

if (app.Environment.IsDevelopment())
{
}

app.Run();