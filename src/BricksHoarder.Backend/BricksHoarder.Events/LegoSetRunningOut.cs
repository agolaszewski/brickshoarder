using BricksHoarder.Core.Events;

namespace BricksHoarder.Events
{
    public record LegoSetRunningOut(string SetId, DateTime RunningOutSince) : IEvent;
}