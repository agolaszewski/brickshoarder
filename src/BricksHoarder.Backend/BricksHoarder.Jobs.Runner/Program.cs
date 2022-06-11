using BricksHoarder.Jobs;
using Microsoft.Extensions.Configuration;
using RebrickableApi;

try
{
    var config = new ConfigurationBuilder()
        .AddUserSecrets<Program>()
        .Build();

    var httpClient = new HttpClient();
    httpClient.BaseAddress = new Uri("https://rebrickable.com");
    httpClient.DefaultRequestHeaders.Add("Authorization", $"key {config["RebrickableApi:Key"]}");

    var job = new SyncSetsJob(new RebrickableClient(httpClient));
    await job.Run(new SyncSetsJobInput()
    {
        PageNumber = 1
    });
}
catch(Exception ex)
{
    Console.WriteLine(ex);
}
