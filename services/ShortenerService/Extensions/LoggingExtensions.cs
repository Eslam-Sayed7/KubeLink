using Seq.Extensions.Logging;
using Serilog;

namespace ShortenerService.Extensions;

public static class LoggingExtensions
{
    public static void ConfigureLogging(this IHostBuilder host)
    {
        try
        {
            SelfLog.Enable(Console.Error);

            var logDirectory = Path.Combine(Directory.GetParent(AppContext.BaseDirectory)!.Parent!.Parent!.Parent!.FullName, "Logs");
            
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            host.UseSerilog((context, loggerConfig) =>
            {
                loggerConfig.ReadFrom.Configuration(context.Configuration);
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while configuring logging: {ex.Message}");
        }
    }
}