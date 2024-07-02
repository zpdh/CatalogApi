using System.Collections.Concurrent;

namespace CatalogApi.Logging;

public class CustomerLoggerProvider : ILoggerProvider
{
    readonly CustomerLoggerProviderConfiguration LoggerConfig;
    readonly ConcurrentDictionary<string, CustomerLogger> Loggers = new ConcurrentDictionary<string, CustomerLogger>();

    public CustomerLoggerProvider(CustomerLoggerProviderConfiguration loggerConfig)
    {
        LoggerConfig = loggerConfig;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return Loggers.GetOrAdd(categoryName, logger => new CustomerLogger(logger, LoggerConfig));
    }

    public void Dispose()
    {
        Loggers.Clear();
    }
}
