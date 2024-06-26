﻿using BricksHoarder.Core.Helpers;

namespace BricksHoarder.Core.Aggregates
{
    public interface IAggregateRoot : IBaseAggregateRoot
    {
        IServiceProvider Context { get; set; }
    }
}