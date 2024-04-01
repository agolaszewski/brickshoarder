using System.Security.Principal;
using BricksHoarder.Events.Metadata;

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
}