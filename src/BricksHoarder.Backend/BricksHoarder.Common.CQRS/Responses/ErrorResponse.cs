using System.Net;

namespace BricksHoarder.Common.CQRS.Responses;

public record ErrorResponse : Response
{
    public ErrorResponse(string exceptionId)
    {
        Id = exceptionId;
        StatusCode = HttpStatusCode.InternalServerError;
    }
}