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
            ErrorMessage = message,
            ValidationErrors = errors
        };
    }
}