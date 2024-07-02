using Microsoft.AspNetCore.Mvc.Filters;

namespace CatalogApi.Filters;

//Made for testing purposes, no actual use

public class ApiLoggingFilter : IAsyncActionFilter
{
    private readonly ILogger<ApiLoggingFilter> _logger;

    public ApiLoggingFilter(ILogger<ApiLoggingFilter> logger)
    {
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        _logger.LogInformation("----------------------------------");
        _logger.LogInformation($"{DateTime.Now.ToLongDateString()}");
        _logger.LogInformation($"Start of log");

        await next();

        _logger.LogInformation($"{DateTime.Now.ToLongDateString()}");
        _logger.LogInformation($"End of log");
        _logger.LogInformation("----------------------------------");
    }
}
