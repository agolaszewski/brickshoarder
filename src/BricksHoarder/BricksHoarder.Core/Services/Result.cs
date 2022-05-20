namespace BricksHoarder.Core.Services
{
    public class Result
    {
        public Result()
        {
            IsSuccess = true;
        }

        public Result(IEnumerable<string> errors)
        {
            IsSuccess = false;
            Errors = errors;
        }

        public bool IsSuccess { get; set; }
        public IEnumerable<string> Errors { get; set; }
    }
}
