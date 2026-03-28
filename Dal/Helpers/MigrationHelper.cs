using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Dal.Helpers;

public static class MigrationHelper
{
    public static DbContextOptions CreateOptions()
    {
        var optionsBuilder = new DbContextOptionsBuilder();
        var assemblyName = Assembly.GetExecutingAssembly().FullName;
        optionsBuilder
            .UseNpgsql("Server=; Database=; User Id=; Password=", b => b.MigrationsAssembly(assemblyName))
            .UseSnakeCaseNamingConvention();
        return optionsBuilder.Options;
    }
}