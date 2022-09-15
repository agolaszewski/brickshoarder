using BricksHoarder.Common.CQRS;
using BricksHoarder.Core.Events;
using BricksHoarder.Core.Services;
using MassTransit;
using MassTransit.Transports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Hosting.Internal.HostingApplication;

namespace BricksHoarder.AzureServiceBus
{
    public class IntegrationEventsDispatcher : IIntegrationEventDispatcher
    {
        private readonly IIntegrationEventsQueue _queue;

        public IntegrationEventsDispatcher(IIntegrationEventsQueue queue)
        {
            _queue = queue;
        }

        public async Task DispatchAsync(ConsumeContext context)
        {
            await context.PublishBatch(_queue.Events);
        }
    }
}
