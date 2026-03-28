namespace WebApi;

public class ConfigurationWebHostSetup : IWebHostSetup
{
    public void Invoke(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(
            (_, config) =>
            {
                var currentDirectory = Directory.GetCurrentDirectory();
                var configPath = Path.Combine(
                    currentDirectory[..(currentDirectory.IndexOf("InsmaScheduleConstructor", StringComparison.Ordinal)
                                        + "InsmaScheduleConstructor".Length)],
                    "./Config/");
                var configFiles = Directory.GetFiles(configPath, "*.json");
                foreach (var configFile in configFiles)
                {
                    config.AddJsonFile(configFile, optional: false, reloadOnChange: false);
                }

                config.AddEnvironmentVariables();
            });
    }
}