namespace WebApi;

public class StartupWebHostSetup<TStartup> : IWebHostSetup
    where TStartup : class
{
    public void Invoke(IWebHostBuilder builder)
    {
        builder.UseStartup<TStartup>();
    }
}