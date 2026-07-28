using GeoBingo.Api.Authentication;
using GeoBingo.Api.Health;
using GeoBingo.Api.Hubs;
using GeoBingo.Api.Lobbies;
using GeoBingo.Api.Lobbies.Runtime;
using GeoBingo.Api.Mapping;
using GeoBingo.Api.Middleware;
using GeoBingo.Api.OpenAPI;
using GeoBingo.Api.Serialization;
using GeoBingo.Data;
using GeoBingo.GameModes;
using GeoBingo.GameModes.Registry;
using GeoBingo.Observability;
using GeoBingo.Observability.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Formatting.Compact;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new CompactJsonFormatter())
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddGeoBingoObservability();

    builder.Services.AddGeoBingoData(builder.Configuration);
    builder.Services.AddGeoBingoAuthentication(builder.Configuration);
    builder.Services.AddAuthorization();
    builder.Services.AddGeoBingoGameModes();
    builder.Services.AddGeoBingoLobbyRuntime();

    builder.Services.AddProblemDetails();
    builder.Services.AddSingleton<HubContractValidator>();
    builder.Services.AddSingleton<
        HubInvocationMetadataAccessor>();
    builder.Services.AddSingleton<SignalRErrorMapper>();
    builder.Services.AddSingleton<GeoBingoHubFilter>();
    builder.Services
        .AddHealthChecks()
        .AddCheck<GeoBingoReadinessHealthCheck>(
            "geobingo_ready",
            failureStatus: HealthStatus.Unhealthy,
            tags: ["ready"]);

    builder.Services
        .AddControllers()
        .AddJsonOptions(options => GeoBingoJsonOptions.Configure(options.JsonSerializerOptions));

    builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options => GeoBingoJsonOptions.Configure(options.SerializerOptions));
    builder.Services
        .AddSignalR(options =>
        {
            options.EnableDetailedErrors = false;
            options.MaximumParallelInvocationsPerClient = 1;
            options.AddFilter<GeoBingoHubFilter>();
        })
        .AddJsonProtocol(options => GeoBingoJsonOptions.Configure(options.PayloadSerializerOptions));

    builder.Services.AddOpenApi(
        "v1",
        options => options.AddSchemaTransformer<StringEnumSchemaTransformer>());

    // mapping
    builder.Services.AddSingleton<AuthProviderMapper>()
        .AddSingleton<GameModeMapper>();

    var app = builder.Build();
    _ = app.Services.GetRequiredService<IGameModeRegistry>();
    var lobbyRegistry =
        app.Services.GetRequiredService<ILobbyRegistry>();
    app.Lifetime.ApplicationStopping.Register(
        lobbyRegistry.StopAcceptingCreations);

    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseGeoBingoRequestLogging();
    app.UseCors(GeoBingoAuthenticationExtensions.CorsPolicy);
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHub<GameHub>("/hubs/game");
    app.MapHealthChecks(
        "/health/live",
        new HealthCheckOptions
        {
            Predicate = _ => false
        });
    app.MapHealthChecks(
        "/health/ready",
        new HealthCheckOptions
        {
            Predicate = registration =>
                registration.Tags.Contains("ready")
        });

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }

    app.Run();
}
catch (System.Exception exception)
{
    Log.Fatal(exception, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
