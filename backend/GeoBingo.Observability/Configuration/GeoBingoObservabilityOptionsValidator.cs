using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace GeoBingo.Observability.Configuration;

public sealed class GeoBingoObservabilityOptionsValidator : IValidateOptions<GeoBingoObservabilityOptions>
{
    public ValidateOptionsResult Validate(string? name, GeoBingoObservabilityOptions options)
    {
        if (!options.Otlp.Enabled)
            return ValidateOptionsResult.Success;

        var failures = new List<string>();
        var endpoint = options.Otlp.Endpoint;
        if (string.IsNullOrWhiteSpace(endpoint))
            failures.Add("Enabled OTLP export requires Observability:Otlp:Endpoint or OTEL_EXPORTER_OTLP_ENDPOINT.");
        else if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) || string.IsNullOrWhiteSpace(uri.Host) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            failures.Add("The OTLP endpoint must be an absolute HTTP or HTTPS URI.");

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
