using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace GeoBingo.Api.Configuration;

public sealed class GeoBingoAuthenticationOptionsValidator : IValidateOptions<GeoBingoAuthenticationOptions>
{
    public ValidateOptionsResult Validate(string name, GeoBingoAuthenticationOptions options)
    {
        var failures = new List<string>();

        var google = options.Provider.Google;
        if (string.IsNullOrWhiteSpace(google.ClientId) || string.IsNullOrWhiteSpace(google.ClientSecret))
            failures.Add("Google authentication requires both ClientId and ClientSecret to be configured.");

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
