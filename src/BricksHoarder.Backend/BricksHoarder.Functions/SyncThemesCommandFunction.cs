using BricksHoarder.AzureServiceBus;
using BricksHoarder.Commands.Themes;
using MassTransit;
using Microsoft.Azure.WebJobs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BricksHoarder.Functions;

public class SyncThemesCommandFunction
{
    private readonly IMessageReceiver _receiver;

    public SyncThemesCommandFunction(IMessageReceiver receiver)
    {
        _receiver = receiver;
    }

    [FunctionName($"{nameof(SyncThemesCommand)}Consumer")]
    public async Task Run([ServiceBusTrigger(nameof(SyncThemesCommand), Connection = "ServiceBusConnectionString")] Azure.Messaging.ServiceBus.ServiceBusReceivedMessage command, CancellationToken cancellationToken)
    {
        try
        {
            await _receiver.HandleConsumer<CommandConsumer<SyncThemesCommand>>(nameof(SyncThemesCommand), command, cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}