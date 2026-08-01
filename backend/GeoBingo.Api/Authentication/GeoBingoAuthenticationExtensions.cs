using GeoBingo.Api.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GeoBingo.Api.Authentication;

public static class GeoBingoAuthenticationExtensions
{
    public const string CorsPolicy = "GeoBingo.Frontend";

    public static IServiceCollection AddGeoBingoAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var section = configuration.GetRequiredSection(GeoBingoAuthenticationOptions.SectionName);
        var configured = section.Get<GeoBingoAuthenticationOptions>() ?? new();

        services.AddOptions<GeoBingoAuthenticationOptions>()
            .Bind(section)
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<GeoBingoAuthenticationOptions>, GeoBingoAuthenticationOptionsValidator>();
        services.AddSingleton<IExternalAuthProviderCatalog, ExternalAuthProviderCatalog>();
        services.AddCors(options => options.AddPolicy(
            CorsPolicy,
            policy => policy
                .WithOrigins(configured.AllowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()));

        var authentication = services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = AuthSchemes.Session;
            options.DefaultSignInScheme = AuthSchemes.Session;
            options.DefaultChallengeScheme = AuthSchemes.Session;
        });

        authentication.AddCookie(AuthSchemes.Session, options =>
        {
            options.Cookie.Name = configured.Session.AuthenticationScheme;
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.SlidingExpiration = true;
        });

        authentication.AddGeoBingoGoogleAuthentication(configured.Provider.Google);
        return services;
    }
}
