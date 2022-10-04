using System.Net;
using FluentValidation.Results;

namespace BricksHoarder.Common.CQRS.Responses;

public record Response
{
    public Response()
    {
    }

    public Response(string id)
    {
        Id = id;
        StatusCode = HttpStatusCode.OK;
    }

    public string Id { get; set; }

    public IEnumerable<ValidationFailure> Errors { get; set; } = new List<ValidationFailure>();

    public HttpStatusCode StatusCode { get; set; }
}