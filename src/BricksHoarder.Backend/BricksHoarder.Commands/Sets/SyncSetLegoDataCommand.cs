using BricksHoarder.Core.Commands;

namespace BricksHoarder.Commands.Sets
{
    public record SyncSetLegoDataCommand(string Id) : ICommand;
}