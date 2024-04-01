using BricksHoarder.Core.Commands;

namespace BricksHoarder.Commands.Sets
{
    public record SyncSetRebrickableDataCommand(string SetId) : ICommand;
}