namespace GeoBingo.Observability.Configuration;

public sealed class GeoBingoObservabilityOptions
{
    public const string SectionName = "Observability";

    public GeoBingoOtlpOptions Otlp { get; set; } = new();
}

public sealed class GeoBingoOtlpOptions
{
    public bool Enabled { get; set; }
    public string Endpoint { get; set; } = string.Empty;
}
