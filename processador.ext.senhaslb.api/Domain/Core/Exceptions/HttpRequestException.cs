using Domain.Core.Base;
using System.Net;

namespace Domain.Core.Exceptions
{
    public class HttpRequestException : Exception
    {
        public BaseError? Error { get; private set; }
        public HttpStatusCode HttpReturnCode { get; private set; }

        public HttpRequestException(string? message) : base(message)
        {
            HttpReturnCode = HttpStatusCode.BadRequest;
            Error = new BaseError
            {
                message = message,
                code = "400",
                info = "Exception"
            };
        }

        public HttpRequestException(List<string> errors, string? message = null) : base(message)
        {
            HttpReturnCode = HttpStatusCode.BadRequest;
            var _message = "";

            foreach (var item in errors)
            {
                _message += $" Error: {item} /n/r";
            }

            Error = new BaseError
            {
                message = _message,
                code = "400",
                info = "Header Exeception"
            };
        }

        public HttpRequestException() { }
    }
}