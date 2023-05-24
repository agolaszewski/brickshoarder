using BricksHoarder.Core.Events;

namespace BricksHoarder.Events
{
    public record SyncSagaStarted(string Id) : IEvent;
}