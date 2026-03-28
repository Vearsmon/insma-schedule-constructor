using System.Reflection;
using System.Text;
using Services.Ioc;

namespace WebApi;

public class Startup(IConfiguration configuration) : WebApiStartup(configuration)
{
    public override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.AddSwaggerGen(options =>
        {
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
        });

        services.WithServices(Configuration);
    }

    public override void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        base.Configure(app, env);
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        app.UseEndpoints(routes => routes.MapControllers());
    }
}