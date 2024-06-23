using BricksHoarder.Core.Commands;

namespace BricksHoarder.Commands.Sets
{
    public partial record SyncSetRebrickableDataCommand(string SetId) : ICommand;
}