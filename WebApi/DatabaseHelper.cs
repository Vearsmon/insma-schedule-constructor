using Microsoft.EntityFrameworkCore;

namespace WebApi;

public static class DatabaseHelper
{
    public static DbContextOptions CreateDbContextOptions(IConfiguration configuration, IServiceProvider serviceProvider)
    {
        return CreateDbContextOptions(configuration.GetConnectionString("InsmaScheduleConstructor")!, serviceProvider);
    }

    private static DbContextOptions CreateDbContextOptions(string connectionString, IServiceProvider serviceProvider)
    {
        var optionsBuilder = new DbContextOptionsBuilder()
            // .UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>())
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .EnableSensitiveDataLogging();

        return optionsBuilder.Options;
    }
}