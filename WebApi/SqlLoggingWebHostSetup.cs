using Npgsql;

namespace WebApi;

public class SqlLoggingWebHostSetup : IWebHostSetup
{
    public void Invoke(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            var sqlLoggingEnable = configurationBuilder.Build().GetValue<bool>("SqlLoggingOptions:Enable");
            if (sqlLoggingEnable)
            {
                var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder.AddConsole());
                NpgsqlLoggingConfiguration.InitializeLogging(loggerFactory, parameterLoggingEnabled: true);
            }
        });
    }
}