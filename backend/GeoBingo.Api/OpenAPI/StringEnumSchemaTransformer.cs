using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace GeoBingo.Api.OpenAPI;

internal sealed class StringEnumSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        var contractType =
            Nullable.GetUnderlyingType(context.JsonTypeInfo.Type)
            ?? context.JsonTypeInfo.Type;

        if (!contractType.IsEnum)
            return Task.CompletedTask;

        schema.Type = JsonSchemaType.String;
        schema.Format = null;
        schema.Enum = Enum.GetNames(contractType)
            .Select(name => JsonValue.Create(
                JsonNamingPolicy.SnakeCaseUpper.ConvertName(name))!)
            .Cast<JsonNode>()
            .ToList();

        return Task.CompletedTask;
    }
}
