using System;
using System.Collections.Generic;
using GeoBingo.Observability.Configuration;
using GeoBingo.Observability.Logging;
using GeoBingo.Observability.Metrics;
using GeoBingo.Observability.Tracing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

namespace GeoBingo.Observability;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddGeoBingoObservability(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var section = builder.Configuration.GetSection(GeoBingoObservabilityOptions.SectionName);
        var configured = section.Get<GeoBingoObservabilityOptions>() ?? new();

        builder.Services.AddOptions<GeoBingoObservabilityOptions>()
            .Bind(section)
            .ValidateOnStart();
        builder.Services.AddSingleton<IValidateOptions<GeoBingoObservabilityOptions>, GeoBingoObservabilityOptionsValidator>();

        var resourceAttributes = GeoBingoTelemetry.CreateResourceAttributes(builder.Environment.EnvironmentName);

        builder.Services.AddSingleton<GameMetrics>();
        builder.Services.AddSingleton<GameActivitySource>();

        var otlpEndpoint = configured.Otlp.Enabled
            && Uri.TryCreate(configured.Otlp.Endpoint, UriKind.Absolute, out var endpoint)
                ? endpoint
                : null;

        builder.Services.AddSerilog((services, loggerConfiguration) =>
        {
            loggerConfiguration
                .ReadFrom.Configuration(builder.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithProperty(LoggingConstants.ServiceName, GeoBingoTelemetry.ServiceName)
                .Enrich.WithProperty(LoggingConstants.Environment, builder.Environment.EnvironmentName);

            if (otlpEndpoint is not null)
                loggerConfiguration.WriteTo.OpenTelemetry(options =>
                {
                    options.Endpoint = otlpEndpoint.AbsoluteUri;
                    options.ResourceAttributes = new Dictionary<string, object>(resourceAttributes);
                    options.OnBeginSuppressInstrumentation = SuppressInstrumentationScope.Begin;
                },
                ignoreEnvironment: true);
        });

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(
                serviceName: GeoBingoTelemetry.ServiceName,
                serviceVersion: GeoBingoTelemetry.ServiceVersion,
                serviceInstanceId: GeoBingoTelemetry.ServiceInstanceId)
            .AddAttributes([
                new KeyValuePair<string, object>(
                    "deployment.environment.name",
                    builder.Environment.EnvironmentName)]))
            .WithTracing(tracing =>
            {
                tracing.AddSource(GeoBingoTelemetry.ActivitySourceName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddNpgsql();

                if (otlpEndpoint is not null)
                    tracing.AddOtlpExporter(options => options.Endpoint = otlpEndpoint);
            })
            .WithMetrics(metrics =>
            {
                metrics.AddMeter(GeoBingoTelemetry.MeterName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation()
                    .AddNpgsqlInstrumentation(_ => { });

                if (otlpEndpoint is not null)
                    metrics.AddOtlpExporter(options => options.Endpoint = otlpEndpoint);
            });

        return builder;
    }
}
