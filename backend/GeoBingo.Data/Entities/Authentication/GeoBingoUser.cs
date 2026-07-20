using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GeoBingo.Data.Entities.Authentication;

[Table("users", Schema = "auth")]
[Index(nameof(Email), IsUnique = true)]
[Index(nameof(Handle), IsUnique = true)]
public sealed class GeoBingoUser
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Column("email")]
    [EmailAddress]
    [MaxLength(320)]
    public string? Email { get; set; }

    [Column("handle")]
    [Required]
    [MaxLength(64)]
    public required string Handle { get; set; }

    [Column("display_name")]
    [Required]
    [MaxLength(64)]
    public required string DisplayName { get; set; }

    [Column("avatar_url")]
    [Url]
    public string? AvatarUrl { get; set; }

    [Column("role")]
    [Required]
    public UserRole Role { get; set; } = UserRole.User;

    [Column("created_at")]
    [Required]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Column("updated_at")]
    [Required]
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    [InverseProperty(nameof(UserAuthIdentityEntity.User))]
    public ICollection<UserAuthIdentityEntity> Identities { get; } = [];
}
