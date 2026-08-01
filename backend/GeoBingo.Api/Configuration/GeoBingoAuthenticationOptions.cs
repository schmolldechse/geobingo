namespace GeoBingo.Api.Configuration;

public sealed class GeoBingoAuthenticationOptions
{
    public const string SectionName = "Authentication";
    public ProviderOptions Provider { get; init; } = new();
    public SessionOptions Session { get; init; } = new();
    public string[] AllowedOrigins { get; init; } = [];
}

public sealed class SessionOptions
{
    public string AuthenticationScheme { get; init; } = "GeoBingo.Auth";
}

public sealed class ProviderOptions
{
    public GoogleProviderOptions Google { get; init; } = new();
}

public sealed class GoogleProviderOptions
{
    public string ClientId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;
    public string CallbackPath { get; init; } = "/api/auth/callback/google";
}