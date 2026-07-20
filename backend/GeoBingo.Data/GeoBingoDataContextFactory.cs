using GeoBingo.Data.Entities.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GeoBingo.Data;

public sealed class GeoBingoDataContextFactory : IDesignTimeDbContextFactory<DataContext>
{
    public DataContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=geobingo;Username=geobingo;Password=password",
            options => options.MapEnum<UserRole>("user_roles", "auth"));
        return new DataContext(optionsBuilder.Options);
    }
}