using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
