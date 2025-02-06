using System.Net;

namespace JobSearchApp.Core.Exceptions;

public class ExceptionWithStatusCode(string message, HttpStatusCode statusCode) : Exception(message)
{
    public HttpStatusCode StatusCode { get; set; } = statusCode;
}