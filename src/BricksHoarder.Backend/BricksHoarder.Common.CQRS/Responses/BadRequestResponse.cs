using System.Net;
using FluentValidation;

namespace BricksHoarder.Common.CQRS.Responses;

public record BadRequestResponse : Response
{
    public BadRequestResponse(ValidationException validationException)
    {
        Errors = validationException.Errors;
        StatusCode = HttpStatusCode.BadRequest;
    }
}