using Dal.Helpers;
using Microsoft.EntityFrameworkCore.Design;

namespace Dal;

public class ContextFactory : IDesignTimeDbContextFactory<InsmaScheduleContext>
{
    public InsmaScheduleContext CreateDbContext(string[] args) => new(MigrationHelper.CreateOptions());
}