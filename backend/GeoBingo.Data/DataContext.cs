using GeoBingo.Data.Entities.Authentication;
using Microsoft.EntityFrameworkCore;

namespace GeoBingo.Data;

public sealed class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
    public DbSet<GeoBingoUser> Users => Set<GeoBingoUser>();
    public DbSet<AuthProviderEntity> AuthProviders => Set<AuthProviderEntity>();
    public DbSet<UserAuthIdentityEntity> UserAuthIdentities => Set<UserAuthIdentityEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
