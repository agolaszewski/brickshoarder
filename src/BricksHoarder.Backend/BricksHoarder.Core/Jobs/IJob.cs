namespace BricksHoarder.Core.Jobs
{
    public interface IJob<in TInput>
    {
        Task RunAsync(TInput input);
    }
}