using System;
using System.Threading;
using System.Threading.Tasks;
using GeoBingo.Data.Entities.Authentication;
using Microsoft.EntityFrameworkCore;

namespace GeoBingo.Data.Repository;

public interface IUserRepository
{
    Task<GeoBingoUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<GeoBingoUser?> GetByHandleAsync(string handle, CancellationToken cancellationToken = default);
    Task<bool> HandleExistsAsync(string handle, CancellationToken cancellationToken = default);
    Task<GeoBingoUser?> UpdateUserAsync(Guid id, GeoBingoUser updatedUser, CancellationToken cancellationToken = default);
}

internal sealed class UserRepository(DataContext dataContext) : IUserRepository
{

    public async Task<GeoBingoUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => await dataContext.Users
        .AsNoTracking()
        .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

    public async Task<GeoBingoUser?> GetByHandleAsync(string handle, CancellationToken cancellationToken = default) => await dataContext.Users
        .AsNoTracking()
        .FirstOrDefaultAsync(user => user.Handle == handle, cancellationToken);

    public async Task<bool> HandleExistsAsync(string handle, CancellationToken cancellationToken = default) => await dataContext.Users
        .AnyAsync(user => user.Handle == handle, cancellationToken);

    public async Task<GeoBingoUser?> UpdateUserAsync(Guid id, GeoBingoUser updatedUser, CancellationToken cancellationToken = default)
    {
        var user = await dataContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user is null) return null;

        user.Email = updatedUser.Email;
        user.Handle = updatedUser.Handle;
        user.DisplayName = updatedUser.DisplayName;
        user.AvatarUrl = updatedUser.AvatarUrl;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await dataContext.SaveChangesAsync();
        return user;
    }
}