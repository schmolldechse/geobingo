using System;
using System.Diagnostics;
using System.Threading;

namespace GeoBingo.Api.Hubs;

internal sealed record HubInvocationMetadata(
    string CorrelationId,
    string TraceId);

internal sealed class HubInvocationMetadataAccessor
{
    private readonly AsyncLocal<HubInvocationMetadata?> current =
        new();

    public HubInvocationMetadata Current =>
        current.Value
        ?? new HubInvocationMetadata(
            Guid.NewGuid().ToString("N"),
            ActivityTraceId
                .CreateRandom()
                .ToString());

    public IDisposable Push(HubInvocationMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        var previous = current.Value;
        current.Value = metadata;
        return new Scope(() => current.Value = previous);
    }

    private sealed class Scope : IDisposable
    {
        private readonly Action dispose;
        private int disposed;

        public Scope(Action dispose)
        {
            this.dispose = dispose;
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref disposed, 1) == 0)
            {
                dispose();
            }
        }
    }
}
