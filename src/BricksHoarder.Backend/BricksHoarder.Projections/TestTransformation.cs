using BricksHoarder.Events;
using Marten.Events;
using Marten.Events.Aggregation;

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
}