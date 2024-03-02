using BricksHoarder.Events;
using Marten.Events.Aggregation;
using System.Security.Principal;
using BricksHoarder.Events.Metadata;
using Marten.Events;

namespace BricksHoarder.Projections
{
    public class Test
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public int Version { get; set; }

        public Test()
        {
            Name = "TESTOWY NAME ";
        }
    }

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