using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Ryder.Api.Client.Exceptions
{
    public class RyderApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string? Content { get; }

        public RyderApiException(string message, HttpStatusCode statusCode, string? content = null)
            : base(message)
        {
            StatusCode = statusCode;
            Content = content;
        }
    }
}
