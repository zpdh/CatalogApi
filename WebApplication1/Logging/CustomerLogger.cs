
namespace CatalogApi.Logging;

public class CustomerLogger : ILogger
{
    readonly string Name;

    readonly CustomerLoggerProviderConfiguration LoggerConfig;

    public CustomerLogger(string name, CustomerLoggerProviderConfiguration configuration)
    {
        Name = name;
        LoggerConfig = configuration;
    }

    //Method not used
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel == LoggerConfig.LogLevel;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        string msg = $"{logLevel.ToString()}: {eventId.Id} - {formatter(state, exception)}";

        string path = Path.GetTempPath() + @"\customer_log.txt";

        using (StreamWriter sw = new StreamWriter(path: path, append: true))
        {
            try
            {
                sw.WriteLine(msg);
                sw.Close();
            }
            catch
            {
                throw;
            }
        }
    }
}
