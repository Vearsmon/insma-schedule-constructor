namespace WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        new WebApiBootstrapper<Startup>().Build(args);
    }
}