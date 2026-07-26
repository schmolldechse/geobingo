using System;
using System.Collections.Generic;
using System.Reflection;

namespace GeoBingo.Observability;

public static class GeoBingoTelemetry
{
    public const string ServiceName = "GeoBingo.Api";
    public const string ActivitySourceName = "GeoBingo.Game";
    public const string MeterName = "GeoBingo.Game";

    public static string ServiceVersion { get; } = ResolveServiceVersion();

    public static string ServiceInstanceId { get; } = $"{Environment.MachineName}:{Environment.ProcessId}";

    public static Dictionary<string, object> CreateResourceAttributes(
        string environmentName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(environmentName);

        return new Dictionary<string, object>
        {
            ["service.name"] = ServiceName,
            ["service.version"] = ServiceVersion,
            ["service.instance.id"] = ServiceInstanceId,
            ["deployment.environment.name"] = environmentName
        };
    }

    private static string ResolveServiceVersion()
    {
        var assembly = Assembly.GetEntryAssembly() ?? typeof(GeoBingoTelemetry).Assembly;
        var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        if (!string.IsNullOrWhiteSpace(informationalVersion))
            return informationalVersion.Split('+', 2)[0];

        return assembly.GetName().Version?.ToString() ?? "0.0.0";
    }
}
