using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GeoBingo.Observability.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace GeoBingo.Api.Middleware;

internal sealed partial class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-ID";

    private readonly RequestDelegate next;
    private readonly ILogger<CorrelationIdMiddleware> logger;

    public CorrelationIdMiddleware(
        RequestDelegate next,
        ILogger<CorrelationIdMiddleware> logger)
    {
        this.next = next
            ?? throw new ArgumentNullException(nameof(next));
        this.logger = logger
            ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var correlationId = ResolveCorrelationId(
            context.Request.Headers[HeaderName]);
        context.Items[LoggingConstants.CorrelationId] =
            correlationId;
        Activity.Current?.SetTag("correlation.id", correlationId);

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using (logger.BeginScope(
                   "{CorrelationId}",
                   correlationId))
        {
            await next(context).ConfigureAwait(false);
        }
    }

    public static string Get(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Items.TryGetValue(
                LoggingConstants.CorrelationId,
                out var value)
            && value is string correlationId
            && !string.IsNullOrWhiteSpace(correlationId)
                ? correlationId
                : Guid.NewGuid().ToString("N");
    }

    private static string ResolveCorrelationId(
        StringValues suppliedValues)
    {
        if (suppliedValues.Count == 1
            && suppliedValues[0] is string supplied
            && CorrelationIdPattern().IsMatch(supplied))
        {
            return supplied;
        }

        return Guid.NewGuid().ToString("N");
    }

    [GeneratedRegex(
        "^[A-Za-z0-9._:-]{1,64}$",
        RegexOptions.CultureInvariant)]
    private static partial Regex CorrelationIdPattern();
}
