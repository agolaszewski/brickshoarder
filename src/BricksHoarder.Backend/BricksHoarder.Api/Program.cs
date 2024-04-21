using BricksHoarder.Azure.ServiceBus;
using BricksHoarder.Commands.Sets;
using BricksHoarder.Common;
using BricksHoarder.Core.Commands;
using BricksHoarder.Credentials;
using BricksHoarder.DateTime.Noda;
using BricksHoarder.Domain;
using BricksHoarder.Marten;
using BricksHoarder.Playwright;
using BricksHoarder.Rebrickable;
using BricksHoarder.Redis;
using BricksHoarder.Serilog;
using BricksHoarder.Websites.Scrappers;
using BricksHoarder.Websites.Scrappers.Lego;
using Microsoft.AspNetCore.Mvc;
using Serilog;

//var builder = WebApplication.CreateBuilder(args);
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    ContentRootPath = AppDomain.CurrentDomain.BaseDirectory,
});

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

    services.AddScrappers();
    services.AddPlaywright();

    services.AddAzureServiceBus2(new AzureServiceBusCredentials(config, "AzureServiceBus"), redisCredentials);
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
        //var response = await legoScrapper.RunProductAsync(new LegoScrapper.LegoSetId(id));
        //return response;
    })
    .WithName("Lego")
    .WithOpenApi();

app.Run();

