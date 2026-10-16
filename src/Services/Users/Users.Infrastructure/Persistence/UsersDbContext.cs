using Microsoft.EntityFrameworkCore;

using Users.Application.Abstractions;

namespace Users.Infrastructure.Persistence;

internal sealed class UsersDbContext(DbContextOptions<UsersDbContext> options) : DbContext(options), IUnitOfWork
{
    Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken) => SaveChangesAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(UsersDbContext).Assembly);
}