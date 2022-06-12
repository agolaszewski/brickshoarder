using System;
using RebrickableApi;
using Shouldly;
using System.Threading.Tasks;
using BricksHoarder.Core.Commands;
using Xunit;

namespace BricksHoarder.Commands.Functional.Tests
{
    public class CommandsTests
    {
        private readonly ICommandHandler<CreateSetCommand> _createSetCommandHandler;

        public CommandsTests(ICommandHandler<CreateSetCommand> createSetCommandHandler)
        {
            _createSetCommandHandler = createSetCommandHandler;
        }

        [Fact]
        public async Task Test1()
        {
            await _createSetCommandHandler.ExecuteAsync(new CreateSetCommand()
            {
                Name = "Back to the Future Time Machine",
                CorrelationId = Guid.NewGuid(),
                LastModifiedDate = new DateTime(2022, 6, 1),
                NumParts = 1872,
                SetImgUrl = null,
                SetNumber = "000",
                ThemeId = 1,
                Year = 2022
            });
        }
    }
}