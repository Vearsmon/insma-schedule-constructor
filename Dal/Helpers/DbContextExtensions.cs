using Dal.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dal.Helpers;

public static class DbContextExtension
{
    public static async Task SoftDeleteAsync<TDbContext, TDbEntity>(
        this TDbContext context,
        Guid[] ids,
        CancellationToken cancellationToken = default)
        where TDbContext : DbContext
        where TDbEntity : class, IDbSoftDeleteEntity
    {
        await context.Set<TDbEntity>()
            .Where(entity => ids.Contains(entity.Id))
            .ExecuteUpdateAsync(s => s.SetProperty(entity => entity.IsDeleted, true),
                cancellationToken);
    }
}