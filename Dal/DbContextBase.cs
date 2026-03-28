using Dal.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Dal;

public class DbContextBase(DbContextOptions options) : DbContext(options)
{
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.SetUnderscoreSnakeConventions();
    }
}