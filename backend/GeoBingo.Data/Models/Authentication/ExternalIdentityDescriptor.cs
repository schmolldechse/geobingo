namespace GeoBingo.Data.Models.Authentication;

public sealed record ExternalIdentityDescriptor(
    string ProviderKey,
    string ProviderDisplayName,
    string ExternalSubject,
    string? Email,
    string DisplayName,
    string? AvatarUrl);
