using BricksHoarder.Core.Commands;

namespace BricksHoarder.Commands.Sets
{
    public partial record SyncSetLegoDataCommand(string SetId) : ICommand;
}