using Marten;
using Microsoft.Azure.Functions.Worker;

namespace BricksHoarder.Functions
{
    public class TestFunction
    {
        private readonly IDocumentStore _client;

        public TestFunction(IDocumentStore client)
        {
            _client = client;
        }

        [Function("Test")]
        public async Task RunAsync([TimerTrigger("10 0 0/1 * * *", RunOnStartup = true)] TimerInfo trigger)
        {
            try
            {
                await _client.Advanced.ResetAllData();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}