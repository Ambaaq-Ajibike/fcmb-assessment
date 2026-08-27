using BankingApi.Api.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
namespace BankingApi.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            var (status, code, title) = exception switch
            {
                ApiException api => (api.StatusCode, api.Code, api.Message),
                SqlException { Number: 2601 or 2627 } => (409, "duplicate_resource", "A resource with these details already exists."),
                _ => (500, "internal_error", "An unexpected error occurred.")
            };
            if (status == 500)
            {
                logger.LogError(exception, "Unhandled exception");
            }
            else
            {
                logger.LogWarning(exception, "Request rejected with {Code}", code);
            }
            context.Response.StatusCode = status;
            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Extensions =
                {
                    ["code"] = code,
                    ["traceId"] = context.TraceIdentifier
                }
            };

            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
