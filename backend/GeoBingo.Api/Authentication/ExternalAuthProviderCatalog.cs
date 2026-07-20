using System;
using System.Collections.Generic;

namespace GeoBingo.Api.Authentication;

public sealed record ExternalAuthProviderDescriptor(
    string Name,
    string DisplayName,
    string AuthenticationScheme);

public interface IExternalAuthProviderCatalog
{
    bool TryGet(string providerName, out ExternalAuthProviderDescriptor descriptor);
    IReadOnlyCollection<ExternalAuthProviderDescriptor> GetAll();
}

internal sealed class ExternalAuthProviderCatalog : IExternalAuthProviderCatalog
{
    private static readonly Dictionary<string, ExternalAuthProviderDescriptor> Providers =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["google"] = new(
                Name: "google",
                DisplayName: "Google",
                AuthenticationScheme: AuthSchemes.Google)
        };

    public IReadOnlyCollection<ExternalAuthProviderDescriptor> GetAll() => Providers.Values;

    public bool TryGet(string name, out ExternalAuthProviderDescriptor provider) => Providers.TryGetValue(name, out provider!);
}
