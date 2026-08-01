using System.Text.Json;
using System.Text.Json.Serialization;

namespace GeoBingo.Api.Serialization;

internal static class GeoBingoJsonOptions
{
    public static void Configure(JsonSerializerOptions options)
    {
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        options.Converters.Add(new JsonStringEnumConverter(
            JsonNamingPolicy.SnakeCaseUpper,
            allowIntegerValues: false));
    }
}
