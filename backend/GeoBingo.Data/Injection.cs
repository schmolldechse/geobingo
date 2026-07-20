using System;
using GeoBingo.Data.Entities.Authentication;
using GeoBingo.Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GeoBingo.Data;

public static class Injection
{
    public static IServiceCollection AddGeoBingoData(
            this IServiceCollection services,
            IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is required.");
        services.AddDbContext<DataContext>(options => options.UseNpgsql(
            connectionString,
            options => options.MapEnum<UserRole>("user_roles", "auth")));

        services.AddScoped<IAuthenticationRepository, AuthenticationRepository>()
            .AddScoped<IUserRepository, UserRepository>();
        return services;
    }
}
