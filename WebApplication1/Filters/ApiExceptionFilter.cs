using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CatalogApi.Filters;

public class ApiExceptionFilter : IExceptionFilter
{
    private readonly ILogger<ApiExceptionFilter> _logger;

    public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
    {
        _logger=logger;
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError("An untreated exception occurred: Status Code 500");

        context.Result = new ObjectResult("There was an error processing your request.")
        {
            StatusCode = StatusCodes.Status500InternalServerError
        };
    }
}
