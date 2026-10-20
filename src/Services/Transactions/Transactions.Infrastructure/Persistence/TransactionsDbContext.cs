using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using Transactions.Application.Abstractions;
using Transactions.Application.Common;

namespace Transactions.Infrastructure.Persistence;

internal sealed class TransactionsDbContext(DbContextOptions<TransactionsDbContext> options)
    : DbContext(options), IUnitOfWork
{
    // SQL Server errors for a duplicate key in a unique index or constraint.
    private static readonly int[] UniqueViolationErrors = [2601, 2627];

    async Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is SqlException sqlException
                  && UniqueViolationErrors.Contains(sqlException.Number))
        {
            // The last line of defence when two requests pass the application's duplicate check at the same time.
            throw new ConflictException("The change conflicts with existing data.");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TransactionsDbContext).Assembly);
}