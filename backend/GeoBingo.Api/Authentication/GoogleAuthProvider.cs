using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using GeoBingo.Api.Configuration;
using GeoBingo.Data.Entities.Authentication;
using GeoBingo.Data.Models.Authentication;
using GeoBingo.Data.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.DependencyInjection;

namespace GeoBingo.Api.Authentication;

public static class GoogleAuthProvider
{
    private const string ProviderName = "google";
    private const string ProviderDisplayName = "Google";

    public static AuthenticationBuilder AddGeoBingoGoogleAuthentication(
        this AuthenticationBuilder authentication,
        GoogleProviderOptions options) => authentication.AddGoogle(AuthSchemes.Google, googleOptions =>
        {
            googleOptions.ClientId = options.ClientId;
            googleOptions.ClientSecret = options.ClientSecret;
            googleOptions.CallbackPath = options.CallbackPath;
            googleOptions.SignInScheme = AuthSchemes.Session;

            googleOptions.Scope.Clear();
            googleOptions.Scope.Add("openid");
            googleOptions.Scope.Add("profile");
            googleOptions.Scope.Add("email");

            googleOptions.ClaimActions.MapJsonKey("urn:google:picture", "picture", "url");
            googleOptions.Events.OnCreatingTicket = PersistGoogleIdentityAsync;
        });

    private static async Task PersistGoogleIdentityAsync(OAuthCreatingTicketContext context)
    {
        var providerSubject = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Google subject claim is required.");
        var email = context.Principal?.FindFirstValue(ClaimTypes.Email);
        var displayName = context.Principal?.FindFirstValue(ClaimTypes.Name) ?? "GeoBingo Player";
        var avatarUrl = context.Principal?.FindFirstValue("urn:google:picture");

        var authenticationRepository = context.HttpContext.RequestServices.GetRequiredService<IAuthenticationRepository>();

        var user = await authenticationRepository.GetOrCreateUserAsync(new ExternalIdentityDescriptor(
            ProviderName,
            ProviderDisplayName,
            providerSubject,
            email,
            displayName,
            avatarUrl
        ));

        context.Principal = CreatePrincipal(user);
    }

    private static ClaimsPrincipal CreatePrincipal(GeoBingoUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString("D")),
            new(ClaimTypes.Name, user.DisplayName),
            new(ClaimTypes.Role, user.Role == UserRole.Admin ? "ADMIN" : "USER"),
            new(GeoBingoClaimTypes.Handle, user.Handle)
        };

        if (!string.IsNullOrWhiteSpace(user.AvatarUrl))
            claims.Add(new Claim(GeoBingoClaimTypes.AvatarUrl, user.AvatarUrl));

        return new ClaimsPrincipal(new ClaimsIdentity(claims, AuthSchemes.Session));
    }
}
