using UI.Models.API;

namespace UI.Services.API
{
    public class APIResult<T>
    {
        public bool IsSuccess { get; set; }
        public T? Data { get; set; }
        public int? StatusCode { get; set; }
        public string? ErrorMessage { get; set; }
        public List<ApiValidationError>? ValidationErrors { get; set; }

        public static APIResult<T> Success(T data) => new APIResult<T>
        {
            IsSuccess = true,
            Data = data
        };

        public static APIResult<T> Failure(int statusCode, string? message = null, List<ApiValidationError>? errors = null) => new APIResult<T>
        {
            IsSuccess = false,
            StatusCode = statusCode,
            ErrorMessage = message ?? GetDefaultMessageForStatusCode(statusCode),
            ValidationErrors = errors
        };

        private static string GetDefaultMessageForStatusCode(int statusCode)
        {
            return statusCode switch
            {
                400 => "Solicitud incorrecta.",
                401 => "No autorizado.",
                404 => "Recurso no encontrado.",
                409 => "Conflicto con el estado actual del recurso.",
                424 => "La operación falló debido a dependencias existentes.",
                500 => "Error interno del servidor.",
                _ => "Error en la petición."
            };
        }
    }
}