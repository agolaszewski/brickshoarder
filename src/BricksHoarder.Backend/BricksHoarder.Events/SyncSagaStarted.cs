﻿using BricksHoarder.Core.Events;

namespace BricksHoarder.Events
{
    public record SyncSagaStarted(Guid Id) : IEvent;
}