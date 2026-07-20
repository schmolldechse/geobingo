using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GeoBingo.Data.Entities.Authentication;

[Table("user_auth_identities", Schema = "auth")]
[Index(nameof(UserId), nameof(AuthProviderId), IsUnique = true)]
public sealed class UserAuthIdentityEntity
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    [InverseProperty(nameof(GeoBingoUser.Identities))]
    public GeoBingoUser User { get; set; } = null!;

    [Column("auth_provider_id")]
    public Guid AuthProviderId { get; set; }

    [ForeignKey(nameof(AuthProviderId))]
    [InverseProperty(nameof(AuthProviderEntity.Identities))]
    public AuthProviderEntity AuthProvider { get; set; } = null!;

    [Column("external_subject")]
    [Required]
    [MaxLength(255)]
    public required string ExternalSubject { get; set; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Column("last_login_at")]
    public DateTimeOffset LastLoginAt { get; set; } = DateTimeOffset.UtcNow;
}