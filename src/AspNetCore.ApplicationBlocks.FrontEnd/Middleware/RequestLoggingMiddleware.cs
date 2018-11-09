using System;
using System.Threading.Tasks;
using SerilogEventLogger;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.ApplicationBlocks.FrontEnd.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly IEventLogger _logger;

        public RequestLoggingMiddleware(IEventLogger<RequestLoggingMiddleware> logger)
        {
            this._logger = logger;
        }

        public async Task Invoke(HttpContext httpContext, Func<Task> next)
        {
            if (httpContext?.TraceIdentifier != null)
            {
                httpContext.Response.Headers["RequestId"] = httpContext.TraceIdentifier;
            }

            var requestData = new
            {
                Connection = new
                {
                    httpContext.Connection.Id,
                    LocalIpAddress = httpContext.Connection.LocalIpAddress.ToString(),
                    httpContext.Connection.LocalPort,
                    RemoteIpAddress = httpContext.Connection.RemoteIpAddress.ToString(),
                    httpContext.Connection.RemotePort,
                },
                User = httpContext.User?.Identity?.Name,
                RequestMethod = httpContext.Request.Method,
                RequestPath = httpContext.Request.Path.Value,
                RequestStatusCode = httpContext.Response?.StatusCode,
                RequestId = httpContext?.TraceIdentifier
            };

            using (_logger.BeginScope(requestData))
            using (var timer = _logger.StartTimer("RequestTime"))
            {
                await next();

                timer.Add("StatusCode", httpContext.Response?.StatusCode);
            }
        }
    }
}