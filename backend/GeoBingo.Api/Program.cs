using GeoBingo.Api.Authentication;
using GeoBingo.Api.Mapping;
using GeoBingo.Api.OpenAPI;
using GeoBingo.Api.Serialization;
using GeoBingo.Data;
using GeoBingo.GameModes;
using GeoBingo.GameModes.Registry;
using GeoBingo.Observability;
using GeoBingo.Observability.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
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

    builder.Services.AddProblemDetails();

    builder.Services
        .AddControllers()
        .AddJsonOptions(options => GeoBingoJsonOptions.Configure(options.JsonSerializerOptions));
    builder.Services.Configure<JsonOptions>(options => GeoBingoJsonOptions.Configure(options.SerializerOptions));
    builder.Services
        .AddSignalR()
        .AddJsonProtocol(options => GeoBingoJsonOptions.Configure(options.PayloadSerializerOptions));

    builder.Services.AddOpenApi(
        "v1",
        options => options.AddSchemaTransformer<StringEnumSchemaTransformer>());

    // mapping
    builder.Services.AddSingleton<AuthProviderMapper>()
        .AddSingleton<GameModeMapper>();

    var app = builder.Build();
    _ = app.Services.GetRequiredService<IGameModeRegistry>();

    app.UseGeoBingoRequestLogging();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

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
