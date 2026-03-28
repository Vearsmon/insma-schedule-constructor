using Microsoft.AspNetCore;

namespace WebApi;

public class WebApiBootstrapper<TStartup> where TStartup : class
{
    private readonly List<IWebHostSetup> _setups =
    [
        new StartupWebHostSetup<TStartup>(),
        new ConfigurationWebHostSetup(),
        new SqlLoggingWebHostSetup()
    ];

    public void Build(string[] args)
    {
        var builder = WebHost.CreateDefaultBuilder(args);

        foreach (var setup in _setups)
        {
            setup.Invoke(builder);
        }

        builder
            .Build()
            .Run();
    }
}