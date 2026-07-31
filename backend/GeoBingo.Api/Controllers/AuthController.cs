using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeoBingo.Api.Authentication;
using GeoBingo.Api.Mapping;
using GeoBingo.Contracts.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace GeoBingo.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    IExternalAuthProviderCatalog providerCatalog,
    AuthProviderMapper authProviderMapper
) : ControllerBase
{
    [HttpGet("providers")]
    [AllowAnonymous]
    [EndpointName("ListAuthenticationProviders")]
    [EndpointSummary("List enabled authentication providers")]
    [EndpointDescription("Returns the registered external authentication providers as public API contracts.")]
    [ProducesResponseType<IReadOnlyList<AuthProvider>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<AuthProvider>> GetProviders()
    {
        var providers = providerCatalog.GetAll()
            .Select(authProviderMapper.Map)
            .ToArray();
        return Ok(providers);
    }

    [HttpGet("login/{providerKey}")]
    [AllowAnonymous]
    [EndpointName("StartExternalAuthentication")]
    [EndpointSummary("Start an external authentication flow")]
    [EndpointDescription("Validates the provider and local return URL, stores one-time server-side state, and returns the provider challenge.")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public IActionResult Login(string providerKey, [FromQuery] string? returnUrl)
    {
        if (!providerCatalog.TryGet(providerKey, out var providerDescriptor))
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Unknown authentication provider",
                detail: "The requested authentication provider is not available.");

        var redirectUrl = !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? Url.Content(returnUrl)
            : "/";

        return Challenge(
            new AuthenticationProperties { RedirectUri = redirectUrl },
            providerDescriptor.AuthenticationScheme);
    }

    [HttpGet("session")]
    [AllowAnonymous]
    [EndpointName("GetCurrentAuthenticationSession")]
    [EndpointSummary("Read the current GeoBingo session")]
    [EndpointDescription("Returns an anonymous result or the authenticated user's public handle/profile projection without the internal user ID.")]
    [ProducesResponseType<Session>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
    public ActionResult<Session> GetSession()
    {
        if (User.Identity?.IsAuthenticated != true)
            return Ok(new Session { Authenticated = false, User = null });

        if (!GeoBingoIdentityClaims.TryRead(User, out var identity) || identity is null)
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Invalid session",
                detail: "The current session could not be validated.");

        return Ok(new Session
        {
            Authenticated = true,
            User = new SessionUser
            {
                Handle = identity.Handle,
                DisplayName = identity.DisplayName,
                AvatarUrl = identity.AvatarUrl
            }
        });
    }

    [HttpPost("logout")]
    [Authorize]
    [EndpointName("EndCurrentAuthenticationSession")]
    [EndpointSummary("End the current GeoBingo session")]
    [EndpointDescription("Signs out the secure cookie session. The request requires an authenticated principal, trusted origin, and valid antiforgery token.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(AuthSchemes.Session);
        return NoContent();
    }
}
