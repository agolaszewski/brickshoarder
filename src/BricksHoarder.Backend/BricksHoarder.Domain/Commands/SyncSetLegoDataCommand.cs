using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BricksHoarder.Commands.Sets
{
    public interface IRetry
    {
        public IEnumerable<TimeSpan> RetryPolicy { get; }
    }

    public partial record SyncSetLegoDataCommand
    {
        public IEnumerable<TimeSpan> RetryPolicy => new List<TimeSpan>()
        {
            TimeSpan.FromMinutes(30),
            TimeSpan.FromHours(24),
        };
    }
}
