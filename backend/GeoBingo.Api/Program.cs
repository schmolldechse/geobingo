using GeoBingo.Api.Authentication;
using GeoBingo.Api.Mapping;
using GeoBingo.Api.OpenAPI;
using GeoBingo.Api.Serialization;
using GeoBingo.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGeoBingoData(builder.Configuration);
builder.Services.AddGeoBingoAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

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
builder.Services.AddSingleton<AuthProviderMapper>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.Run();