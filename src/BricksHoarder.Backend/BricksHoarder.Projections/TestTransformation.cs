using BricksHoarder.Events;
using JasperFx.Core;
using JasperFx.Core.Reflection;
using Marten;
using Marten.Events;
using Marten.Events.Aggregation;
using Marten.Events.Daemon;
using Marten.Events.Daemon.Internals;
using Marten.Services;
using Marten.Subscriptions;

namespace BricksHoarder.Projections
{
    public class TestTransformation : SingleStreamProjection<Test>
    {
        public TestTransformation()
        {
        }

        public void Apply(IEvent<RebrickableSetDataSynced> e, Test view)
        {
            view.Id = e.StreamKey;
            view.Name = e.Data.Name;
            view.Version++;
        }

        public void Apply(RebrickableMinifigureDataSynced e, Test view)
        {
            view.Version = e.Quantity;
        }
    }

    public class TestSubscription : ISubscription
    {
        public ValueTask DisposeAsync()
        {
            return new ValueTask();
        }

        public Task<IChangeListener> ProcessEventsAsync(EventRange page, ISubscriptionController controller, IDocumentOperations operations, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Starting to process events from {page.SequenceFloor} to {page.SequenceCeiling}");
            foreach (var e in page.Events)
            {
                Console.WriteLine($"Got event of type {e.Data.GetType().NameInCode()} from stream {e.StreamKey}");
            }

            // If you don't care about being signaled for
            return Task.FromResult(NullChangeListener.Instance);
        }
    }
}