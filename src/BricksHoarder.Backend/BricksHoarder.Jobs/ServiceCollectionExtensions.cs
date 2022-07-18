using MassTransit;

namespace BricksHoarder.Jobs
{
    public static class ServiceCollectionExtensions
    {
        public static void AddJobsConsumers(this IBusRegistrationConfigurator busRegistrationConfigurator)
        {
            busRegistrationConfigurator.AddConsumer<SyncSetsConsumer>();
        }

        public static void UseJobsConsumers(this IReceiveEndpointConfigurator receiveEndpointConfigurator, IBusRegistrationContext busRegistrationContext)
        {
            receiveEndpointConfigurator.ConfigureConsumer<SyncSetsConsumer>(busRegistrationContext);
        }
    }
}