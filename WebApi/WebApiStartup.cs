using Dal.Transactions;

namespace WebApi;

public class WebApiStartup
{
    protected readonly IConfiguration Configuration;

    protected WebApiStartup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public virtual void ConfigureServices(IServiceCollection services)
    {
        services.Configure<TransactionOptions>(Configuration.GetSection(nameof(TransactionOptions)));

        services.AddControllers();

        services.AddScoped(provider => DatabaseHelper.CreateDbContextOptions(Configuration, provider));
    }

    public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseDefaultFiles()
            .UseStaticFiles();

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseRouting();
    }
}