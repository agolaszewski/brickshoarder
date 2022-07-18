using BricksHoarder.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RebrickableApi;

var serviceProvider = new ServiceCollection();
var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

serviceProvider.AddSingleton<RebrickableClient>(_ =>
{
    var httpClient = new HttpClient();
    httpClient.BaseAddress = new Uri("https://rebrickable.com");
    httpClient.DefaultRequestHeaders.Add("Authorization", $"key {config["RebrickableApi:Key"]}");

    return new RebrickableClient(httpClient);
});

