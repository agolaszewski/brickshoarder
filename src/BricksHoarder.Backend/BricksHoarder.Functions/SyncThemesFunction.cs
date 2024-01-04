using BricksHoarder.Core.Events;
using BricksHoarder.DateTime;
using BricksHoarder.Events;
using BricksHoarder.MassTransit;
using Microsoft.Azure.Functions.Worker;

namespace BricksHoarder.Functions
{
    public class SyncThemesFunction
    {
        private readonly IEventDispatcher _eventDispatcher;
        private readonly IDateTimeProvider _dataTimeProvider;
        private readonly MassTransitDbContext _context;

        public SyncThemesFunction(IEventDispatcher eventDispatcher, IDateTimeProvider dataTimeProvider, MassTransitDbContext context)
        {
            _eventDispatcher = eventDispatcher;
            _dataTimeProvider = dataTimeProvider;
            _context = context;
        }

        [Function("SyncSagaFunction")]
        public async Task RunAsync([TimerTrigger("0 */1 * * * *", RunOnStartup = true)] TimerInfo trigger)
        {
            try
            {
                await _eventDispatcher.DispatchAsync(new SyncSagaStarted(_dataTimeProvider.UtcNow().ToString("yyyyMMdd")));
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}