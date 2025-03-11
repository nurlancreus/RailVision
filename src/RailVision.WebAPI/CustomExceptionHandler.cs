using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;

namespace RailVision.WebAPI
{
    public class CustomExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var problemDetails = GenerateProblemDetails(exception, httpContext);

            httpContext.Response.ContentType = MediaTypeNames.Application.Json;
            httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

            await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                Exception = exception,
                HttpContext = httpContext,
                ProblemDetails = problemDetails
            });

            return true;
        }

        private static ProblemDetails GenerateProblemDetails(Exception exception, HttpContext httpContext)
        {
            int statusCode;

            var problemDetails = new ProblemDetails
            {
                Type = exception.GetType().Name,
                Detail = exception.Message,
                Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}"
            };


            statusCode = exception switch
            {
                ValidationException => StatusCodes.Status400BadRequest,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                KeyNotFoundException => StatusCodes.Status404NotFound,
                NotImplementedException => StatusCodes.Status501NotImplemented,
                _ => StatusCodes.Status500InternalServerError
            };

            problemDetails.Status = statusCode;
            problemDetails.Title = GetTitleForStatusCode(statusCode);

            if (exception is ValidationException validationException)
            {
                problemDetails.Extensions["errors"] = validationException.Data;
            }

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Production")
            {
                problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            }

            return problemDetails;
        }

        private static string GetTitleForStatusCode(int statusCode) => statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad Request",
            StatusCodes.Status401Unauthorized => "Unauthorized",
            StatusCodes.Status404NotFound => "Not Found",
            StatusCodes.Status500InternalServerError => "Internal Server Error",
            StatusCodes.Status501NotImplemented => "Not Implemented",
            _ => "An error occurred"
        };
    }
}
