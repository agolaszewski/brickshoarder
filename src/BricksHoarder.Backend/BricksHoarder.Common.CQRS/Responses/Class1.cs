using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;

namespace BricksHoarder.Common.CQRS.Responses
{
    public record BadRequestResponse : Response
    {
        public BadRequestResponse(ValidationException validationException)
        {
            Errors = validationException.Errors;
            StatusCode = HttpStatusCode.BadRequest;
        }
    }

    public record ErrorResponse : Response
    {
        public ErrorResponse(string exceptionId)
        {
            Id = exceptionId;
            StatusCode = HttpStatusCode.InternalServerError;
        }
    }

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
