using System;
using System.Security.Claims;

namespace GeoBingo.Api.Authentication;

internal sealed record GeoBingoIdentity(
    Guid UserId,
    string Handle,
    string DisplayName,
    string? AvatarUrl);

internal static class GeoBingoIdentityClaims
{
    public static bool TryRead(ClaimsPrincipal? principal, out GeoBingoIdentity? identity)
    {
        identity = null;
        if (principal?.Identity?.IsAuthenticated != true || !TryReadUserId(principal, out var userId))
            return false;

        var handle = principal.FindFirstValue(GeoBingoClaimTypes.Handle);
        var displayName = principal.FindFirstValue(ClaimTypes.Name);
        if (string.IsNullOrWhiteSpace(handle) || string.IsNullOrWhiteSpace(displayName))
            return false;

        identity = new GeoBingoIdentity(
            userId,
            handle,
            displayName,
            principal.FindFirstValue(GeoBingoClaimTypes.AvatarUrl));
        return true;
    }

    public static bool TryReadUserId(ClaimsPrincipal? principal, out Guid userId) =>
        Guid.TryParse(principal?.FindFirstValue(ClaimTypes.NameIdentifier), out userId) && userId != Guid.Empty;
}
