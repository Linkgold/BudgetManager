using System.Net;
using System.Text.Json;


namespace API.Middleware
{
    /// <summary>
    /// Middleware para manejo global de excepciones
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        /// <summary>
        /// Constructor del middleware
        /// </summary>
        /// <param name="next">Siguiente middleware</param>
        /// <param name="logger">Logger</param>
        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Invoca el middleware
        /// </summary>
        /// <param name="context">Contexto HTTP</param>
        public async Task InvokeAsync(HttpContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// Maneja la excepción y devuelve una respuesta apropiada
        /// </summary>
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (exception == null) throw new ArgumentNullException(nameof(exception));

            // Loggear la excepción
            _logger.LogError(exception, "An unhandled exception occurred");

            // Determinar el código de estado y mensaje
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
            string message = "An error occurred while processing your request";

            if (exception is KeyNotFoundException)
            {
                statusCode = HttpStatusCode.NotFound;
                message = exception.Message;
            }
            else if (exception is ArgumentException || exception is InvalidOperationException)
            {
                statusCode = HttpStatusCode.BadRequest;
                message = exception.Message;
            }
            else if (exception is UnauthorizedAccessException)
            {
                statusCode = HttpStatusCode.Unauthorized;
                message = "You are not authorized to perform this action";
            }

            // Crear respuesta
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            ErrorResponse errorResponse = new ErrorResponse
            {
                StatusCode = (int)statusCode,
                Message = message,
                Detail = exception.Message,
                Timestamp = DateTime.UtcNow
            };

            string jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            await context.Response.WriteAsync(jsonResponse);
        }

        /// <summary>
        /// DTO para respuesta de error
        /// </summary>
        private class ErrorResponse
        {
            public int StatusCode { get; set; }
            public string Message { get; set; }
            public string Detail { get; set; }
            public DateTime Timestamp { get; set; }
        }
    }
}