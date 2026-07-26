using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Serilog;
using Serilog.Events;

namespace GeoBingo.Observability.Logging;

public static class RequestLogging
{
    public static IApplicationBuilder UseGeoBingoRequestLogging(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RoutePattern} responded {StatusCode} in {Elapsed:0.0000} ms";
            options.GetLevel = (httpContext, _, exception) =>
            {
                if (exception is not null
                    || httpContext.Response.StatusCode >= 500)
                    return LogEventLevel.Error;

                return httpContext.Response.StatusCode >= 400
                    ? LogEventLevel.Warning
                    : LogEventLevel.Information;
            };
            options.EnrichDiagnosticContext =
                (diagnosticContext, httpContext) =>
                {
                    var routePattern = (httpContext.GetEndpoint() as RouteEndpoint)?.RoutePattern.RawText ?? "unmatched";
                    var activity = Activity.Current;

                    diagnosticContext.Set(LoggingConstants.RoutePattern, routePattern);
                    diagnosticContext.Set(LoggingConstants.RequestPath, httpContext.Request.Path.Value ?? string.Empty);
                    diagnosticContext.Set(LoggingConstants.RequestMethod, httpContext.Request.Method);
                    diagnosticContext.Set(LoggingConstants.StatusCode, httpContext.Response.StatusCode);
                    diagnosticContext.Set(LoggingConstants.TraceId, activity?.TraceId.ToString());
                    diagnosticContext.Set(LoggingConstants.SpanId, activity?.SpanId.ToString());
                    diagnosticContext.Set(LoggingConstants.EventType, "http_request");
                };
        });
    }
}
