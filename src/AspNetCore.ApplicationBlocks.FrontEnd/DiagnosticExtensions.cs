using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DiagnosticAdapter;

namespace AspNetCore.ApplicationBlocks
{
    public static class DiagnosticExtensions
    {
        public static void LogMiddlewareDiagnosticsToConsole(
            this DiagnosticListener diagnosticListener
        )
        {
            var listener = new MiddlewareAnalysisDiagnosticListener();
            diagnosticListener.SubscribeWithAdapter(listener);
        }

        private class MiddlewareAnalysisDiagnosticListener
        {
            [DiagnosticName("Microsoft.AspNetCore.MiddlewareAnalysis.MiddlewareStarting")]
            public virtual void OnMiddlewareStarting(HttpContext httpContext, string name)
            {
                Console.WriteLine($"MiddlewareStarting: {name}; {httpContext.Request.Path}");
            }

            [DiagnosticName("Microsoft.AspNetCore.MiddlewareAnalysis.MiddlewareException")]
            public virtual void OnMiddlewareException(Exception exception, string name)
            {
                Console.WriteLine($"MiddlewareException: {name}; {exception.Message}");
            }

            [DiagnosticName("Microsoft.AspNetCore.MiddlewareAnalysis.MiddlewareFinished")]
            public virtual void OnMiddlewareFinished(HttpContext httpContext, string name)
            {
                Console.WriteLine($"MiddlewareFinished: {name}; {httpContext.Response.StatusCode}");
            }
        }
    }
}