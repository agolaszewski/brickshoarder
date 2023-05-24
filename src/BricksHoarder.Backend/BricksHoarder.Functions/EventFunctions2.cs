using System;
using System.Threading;
using System.Threading.Tasks;
using BricksHoarder.Domain.Sets;
using MassTransit;
using Microsoft.Azure.WebJobs;

namespace BricksHoarder.Functions
{
    public class EventFunctions2
    {
        private readonly IMessageReceiver _receiver;

        public EventFunctions2(IMessageReceiver receiver)
        {
            _receiver = receiver;
        }

        [FunctionName("CommandConsumedSyncThemesCommandConsumer")]
        public async Task Run([ServiceBusTrigger("brickshoarder.events/consumed/SyncThemesCommand", "default", Connection = "ServiceBusConnectionString")] Azure.Messaging.ServiceBus.ServiceBusReceivedMessage @event, CancellationToken cancellationToken)
        {
            try
            {
                await _receiver.HandleSaga<SyncSetsState>("brickshoarder.events/consumed/syncthemescommand", "default", @event, cancellationToken);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}