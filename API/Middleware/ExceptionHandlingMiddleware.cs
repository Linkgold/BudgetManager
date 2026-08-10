using Domain.Exceptions;
using Shared.Models;
using System.Text.Json;

namespace API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IWebHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            int statusCode;
            string message;

            switch (exception)
            {
                case ConflictException _:
                    statusCode = StatusCodes.Status409Conflict;
                    message = exception.Message;
                    break;

                case DependencyException _:
                    statusCode = StatusCodes.Status424FailedDependency;
;
                    message = exception.Message;
                    break;

                case KeyNotFoundException _:
                    statusCode = StatusCodes.Status404NotFound;
                    message = exception.Message;
                    break;

                case ArgumentException _:
                case InvalidOperationException _:
                    statusCode = StatusCodes.Status400BadRequest;
                    message = exception.Message;
                    break;

                case UnauthorizedAccessException _:
                    statusCode = StatusCodes.Status401Unauthorized;
                    message = exception.Message;
                    break;

                case InvalidPasswordException _:
                    statusCode = StatusCodes.Status403Forbidden;
                    message = exception.Message;
                    break;

                default:
                    statusCode = StatusCodes.Status500InternalServerError;
                    message = "Ha ocurrido un error inesperado.";
                    _logger.LogError(exception, "Error no controlado: {Message}", exception.Message);
                    break;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            string? detail = _environment.IsDevelopment() ? exception.StackTrace : null;

            ErrorResponse errorResponse = new ErrorResponse
            {
                StatusCode = statusCode,
                Message = message,
                Detail = exception.StackTrace,
                Timestamp = DateTime.UtcNow
            };

            string jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }
}