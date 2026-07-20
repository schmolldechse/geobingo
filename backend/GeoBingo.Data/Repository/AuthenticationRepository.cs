using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GeoBingo.Data.Entities.Authentication;
using GeoBingo.Data.Models.Authentication;
using Microsoft.EntityFrameworkCore;

namespace GeoBingo.Data.Repository;

public interface IAuthenticationRepository
{
    Task<GeoBingoUser> GetOrCreateUserAsync(ExternalIdentityDescriptor identity, CancellationToken cancellationToken = default);
}

internal sealed class AuthenticationRepository(DataContext dataContext) : IAuthenticationRepository
{
    public async Task<GeoBingoUser> GetOrCreateUserAsync(ExternalIdentityDescriptor identityDescriptor, CancellationToken cancellationToken = default)
    {
        var timestamp = DateTimeOffset.UtcNow;

        var authIdentity = await dataContext.UserAuthIdentities
            .Include(identity => identity.User)
            .Include(identity => identity.AuthProvider)
            .SingleOrDefaultAsync(identity =>
                identity.AuthProvider.ProviderKey == identityDescriptor.ProviderKey &&
                identity.ExternalSubject == identityDescriptor.ExternalSubject);
        if (authIdentity is not null)
        {
            authIdentity.LastLoginAt = timestamp;
            authIdentity.User.UpdatedAt = timestamp;

            if (authIdentity.User.Email is null && identityDescriptor.Email is not null)
                authIdentity.User.Email = identityDescriptor.Email;

            await dataContext.SaveChangesAsync();
            return authIdentity.User;
        }

        var authProvider = await dataContext.AuthProviders
            .SingleOrDefaultAsync(provider => provider.ProviderKey == identityDescriptor.ProviderKey);
        if (authProvider is null)
        {
            authProvider = new AuthProviderEntity
            {
                ProviderKey = identityDescriptor.ProviderKey,
                DisplayName = identityDescriptor.ProviderDisplayName,
                CreatedAt = timestamp
            };
            dataContext.AuthProviders.Add(authProvider);
        }

        var displayName = TrimToMax(
           string.IsNullOrWhiteSpace(identityDescriptor.DisplayName)
               ? "GeoBingo Player"
               : identityDescriptor.DisplayName.Trim(),
           64);

        var user = new GeoBingoUser
        {
            Email = identityDescriptor.Email,
            Handle = await CreateUniqueHandleAsync(
             dataContext,
             identityDescriptor.Email,
             displayName,
             identityDescriptor.ExternalSubject),
            DisplayName = displayName,
            AvatarUrl = TrimNullableToMax(identityDescriptor.AvatarUrl, 512),
            Role = UserRole.User
        };

        authIdentity = new UserAuthIdentityEntity
        {
            User = user,
            AuthProvider = authProvider,
            ExternalSubject = identityDescriptor.ExternalSubject
        };

        dataContext.Users.Add(user);
        dataContext.UserAuthIdentities.Add(authIdentity);

        await dataContext.SaveChangesAsync();
        return user;
    }

    private async Task<string> CreateUniqueHandleAsync(
        DataContext db,
        string? email,
        string displayName,
        string providerSubject)
    {
        var source = email?.Split('@', 2)[0] ?? displayName;
        var normalized = new string(source
            .ToLowerInvariant()
            .Where(char.IsLetterOrDigit)
            .ToArray());

        if (string.IsNullOrWhiteSpace(normalized))
            normalized = "player";

        var suffix = Convert
            .ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(providerSubject)))
            .ToLowerInvariant()[..8];
        var prefix = TrimToMax(normalized, 55);
        var candidate = $"{prefix}-{suffix}";

        if (!await db.Users.AnyAsync(user => user.Handle == candidate))
            return candidate;

        for (var index = 2; index < 100; index++)
        {
            var fallbackPrefix = TrimToMax(prefix, 64 - suffix.Length - index.ToString().Length - 2);
            var fallback = $"{fallbackPrefix}-{suffix}-{index}";

            if (!await db.Users.AnyAsync(user => user.Handle == fallback))
                return fallback;
        }

        return Guid.NewGuid().ToString("N")[..32];
    }

    private string TrimToMax(string value, int maxLength) => value.Length <= maxLength ? value : value[..maxLength];

    private string? TrimNullableToMax(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        return TrimToMax(trimmed, maxLength);
    }
}
