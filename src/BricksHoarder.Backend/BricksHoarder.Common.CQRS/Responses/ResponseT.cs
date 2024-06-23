namespace BricksHoarder.Common.CQRS.Responses
{
    public record Response<TResult> : Response
    {
        public Response()
        {
        }

        public Response(string id, TResult result) : base(id)
        {
            Result = result;
        }

        public TResult Result { get; set; }
    }
}