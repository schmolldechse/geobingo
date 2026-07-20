using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GeoBingo.Data.Entities.Authentication;

[Table("auth_providers", Schema = "auth")]
[Index(nameof(ProviderKey), IsUnique = true)]
public sealed class AuthProviderEntity
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Column("provider_key")]
    [Required]
    [MaxLength(64)]
    public required string ProviderKey { get; set; }

    [Column("display_name")]
    [Required]
    [MaxLength(64)]
    public required string DisplayName { get; set; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [InverseProperty(nameof(UserAuthIdentityEntity.AuthProvider))]
    public ICollection<UserAuthIdentityEntity> Identities { get; } = [];
}
