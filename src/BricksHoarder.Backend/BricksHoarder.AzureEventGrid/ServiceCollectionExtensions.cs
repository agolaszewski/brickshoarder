using Azure;
using Azure.Messaging.EventGrid;
using BricksHoarder.Common.CQRS;
using BricksHoarder.Core.Events;
using BricksHoarder.Credentials;
using Microsoft.Extensions.DependencyInjection;

namespace BricksHoarder.AzureEventGrid
{
    public static class ServiceCollectionExtensions
    {
        public static void AddAzureServiceBus(this IServiceCollection services, AzureEventGridCredentials credentials)
        {
            services.AddSingleton(x => new EventGridPublisherClient(new Uri(credentials.TopicEndpoint),new AzureKeyCredential(credentials.TopicAccessKey)));
            services.AddScoped<IIntegrationEventDispatcher, IntegrationEventsDispatcher>();
            services.AddScoped<IIntegrationEventsQueue, IntegrationEventsQueue>();
        }
    }
}