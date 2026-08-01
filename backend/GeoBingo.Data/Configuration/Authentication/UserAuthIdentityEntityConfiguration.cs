using GeoBingo.Data.Entities.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeoBingo.Data.Configuration.Authentication;

public sealed class UserAuthIdentityEntityConfiguration : IEntityTypeConfiguration<UserAuthIdentityEntity>
{
    public void Configure(EntityTypeBuilder<UserAuthIdentityEntity> builder)
    {
        builder.HasOne(identity => identity.User)
            .WithMany(user => user.Identities)
            .HasForeignKey(identity => identity.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(identity => identity.AuthProvider)
            .WithMany(user => user.Identities)
            .HasForeignKey(identity => identity.AuthProviderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}