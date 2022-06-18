using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BricksHoarder.Core.Commands;

namespace BricksHoarder.Jobs
{
    public record SyncThemesCommand : ICommand
    {
        public int PageNumber { get; set; }

        public Guid CorrelationId { get; set; }
    }
}
