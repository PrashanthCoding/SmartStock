using SmartStock.Application.Common;
using System.Net;
using System.Text.Json;

namespace SmartStock.Web.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "An unhandled exception occurred. Request: {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);

                await HandleExceptionAsync(context, exception);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            if (context.Response.HasStarted)
            {
                throw exception;
            }

            var (statusCode, message) = exception switch
            {
                UnauthorizedAccessException =>
                    (HttpStatusCode.Unauthorized, "Unauthorized access."),

                KeyNotFoundException =>
                    (HttpStatusCode.NotFound, "Requested resource was not found."),

                ArgumentException =>
                    (HttpStatusCode.BadRequest, "Invalid request."),

                InvalidOperationException =>
                    (HttpStatusCode.BadRequest, exception.Message),

                _ =>
                    (HttpStatusCode.InternalServerError,
                        "An unexpected error occurred.")
            };

            context.Response.Clear();
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            var response = new ExceptionResponseDto
            {
                StatusCode = (int)statusCode,
                Message = message,
                Detail = statusCode == HttpStatusCode.InternalServerError
                    ? null
                    : exception.Message,
                TimestampUtc = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(
                response,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

            await context.Response.WriteAsync(json);
        }
    }
}
