namespace WebApi;

public interface IWebHostSetup
{
    void Invoke(IWebHostBuilder builder);
}