using Microsoft.AspNetCore.Components;
using Shared.Models;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using UI.Models.API;
using UI.Services.Interfaces;

namespace UI.Services.API
{
    public class APIService
    {
        private readonly HttpClient _httpClient;
        private readonly LoadingService _loadingService;
        private readonly ILogService _logService;
        private readonly IAuthService _authService;
        private readonly NavigationManager _navigationManager;
        private readonly IToastService _toastService;
        private readonly JsonSerializerOptions _jsonOptions;

        public APIService
        (
            HttpClient httpClient,
            LoadingService loadingService,
            IAuthService authService,
            NavigationManager navigationManager,
            ILogService logService,
            IToastService toastService
        )
        {
            _httpClient = httpClient;
            _loadingService = loadingService;
            _authService = authService;
            _navigationManager = navigationManager;
            _logService = logService;
            _toastService = toastService;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        // ================================================================
        // MÉTODOS GENÉRICOS CON AUTENTICACIÓN
        // ================================================================

        public Task<APIResult<T>> GetAsync<T>(string endpoint) where T : class => ExecuteWithLoadingAsync(() => GetInternalAsync<T>(endpoint), endpoint, "GET");
        public Task<APIResult<List<T>>> GetListAsync<T>(string endpoint) where T : class => ExecuteWithLoadingAsync(() => GetListInternalAsync<T>(endpoint), endpoint, "GET");
        public Task<APIResult<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest request) => ExecuteWithLoadingAsync(() => PostInternalAsync<TRequest, TResponse>(endpoint, request), endpoint, "POST");
        public Task<APIResult<bool>> PostNoContentAsync<TRequest>(string endpoint, TRequest request) where TRequest : class => ExecuteWithLoadingAsync(() => PostNoContentInternalAsync(endpoint, request), endpoint, "POST");
        public Task<APIResult<TResponse>> PutAsync<TRequest, TResponse>(string endpoint, TRequest request) => ExecuteWithLoadingAsync(() => PutInternalAsync<TRequest, TResponse>(endpoint, request), endpoint, "PUT");
        public Task<APIResult<bool>> DeleteAsync(string endpoint) => ExecuteWithLoadingAsync(() => DeleteInternalAsync(endpoint), endpoint, "DELETE");
        public Task<APIResult<T>> DeleteAsync<T>(string endpoint, object? request = null) where T : class => ExecuteWithLoadingAsync(() => DeleteInternalAsync<T>(endpoint, request), endpoint, "DELETE");

        // ================================================================
        // MÉTODOS INTERNOS (LÓGICA DE NEGOCIO SIN TRY-CATCH)
        // ================================================================

        private async Task<APIResult<T>> GetInternalAsync<T>(string endpoint) where T : class
        {
            HttpResponseMessage response = await GetCall(endpoint);

            if (response.IsSuccessStatusCode)
            {
                T? data = await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
                return APIResult<T>.Success(data!);
            }

            ErrorResponse? error = await ParseErrorResponse(response);
            return APIResult<T>.Failure((int)response.StatusCode, error?.Message);
        }

        private async Task<APIResult<List<T>>> GetListInternalAsync<T>(string endpoint) where T : class
        {
            HttpResponseMessage response = await GetCall(endpoint);

            if (response.IsSuccessStatusCode)
            {
                List<T>? data = await response.Content.ReadFromJsonAsync<List<T>>(_jsonOptions);
                return APIResult<List<T>>.Success(data ?? new List<T>());
            }

            ErrorResponse? error = await ParseErrorResponse(response);
            return APIResult<List<T>>.Failure((int)response.StatusCode, error?.Message);
        }

        private async Task<APIResult<TResponse>> PostInternalAsync<TRequest, TResponse>(string endpoint, TRequest request)
        {
            HttpResponseMessage response = await PostCall(endpoint, request);

            if (response.IsSuccessStatusCode)
            {
                TResponse? data = await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions);
                return APIResult<TResponse>.Success(data!);
            }

            ErrorResponse? error = await ParseErrorResponse(response);
            return APIResult<TResponse>.Failure((int)response.StatusCode, error?.Message);
        }

        private async Task<APIResult<bool>> PostNoContentInternalAsync<TRequest>(string endpoint, TRequest request) where TRequest : class
        {
            HttpResponseMessage response = await PostCall(endpoint, request);

            if (response.IsSuccessStatusCode)
            {
                return APIResult<bool>.Success(true);
            }

            ErrorResponse? error = await ParseErrorResponse(response);
            return APIResult<bool>.Failure((int)response.StatusCode, error?.Message);
        }

        private async Task<APIResult<TResponse>> PutInternalAsync<TRequest, TResponse>(string endpoint, TRequest request)
        {
            HttpResponseMessage response = await PutCall(endpoint, request);

            if (response.IsSuccessStatusCode)
            {
                TResponse? data = await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions);
                return APIResult<TResponse>.Success(data!);
            }

            ErrorResponse? error = await ParseErrorResponse(response);
            return APIResult<TResponse>.Failure((int)response.StatusCode, error?.Message);
        }

        private async Task<APIResult<bool>> DeleteInternalAsync(string endpoint)
        {
            HttpResponseMessage response = await DeleteCall(endpoint);

            if (response.IsSuccessStatusCode)
            {
                return APIResult<bool>.Success(true);
            }

            ErrorResponse? error = await ParseErrorResponse(response);
            return APIResult<bool>.Failure((int)response.StatusCode, error?.Message);
        }

        private async Task<APIResult<T>> DeleteInternalAsync<T>(string endpoint, object? request = null) where T : class
        {
            HttpRequestMessage httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(endpoint, UriKind.Relative)
            };

            if (request != null)
            {
                string json = JsonSerializer.Serialize(request, _jsonOptions);
                httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            HttpResponseMessage response = await DeleteCall(httpRequest);

            if (response.IsSuccessStatusCode)
            {
                T? data = await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
                return APIResult<T>.Success(data!);
            }

            ErrorResponse? error = await ParseErrorResponse(response);
            return APIResult<T>.Failure((int)response.StatusCode, error?.Message);
        }


        // ================================================================
        // MÉTODOS PÚBLICOS
        // ================================================================

        public async Task LogoutAsync() => await _authService.LogoutAsync();

        public void NotifySuccess(string message) => _toastService.ShowSuccess(message);
        public void NotifyError(string message) => _toastService.ShowError(message);

        // ================================================================
        // WRAPPER CON LOADING + GESTIÓN DE ERRORES CENTRALIZADA
        // ================================================================

        private async Task<APIResult<T>> ExecuteWithLoadingAsync<T>(Func<Task<APIResult<T>>> action, string endpoint, string operation)
        {
            _loadingService.Show();

            try
            {
                return await action();
            }
            catch (UnauthorizedAccessException)
            {
                throw; // Lo relanzamos para que lo maneje el AuthenticationStateProvider
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"Error en {operation} {endpoint}", ex);
                return APIResult<T>.Failure(500, "Ocurrió un error inesperado.");
            }
            finally
            {
                _loadingService.Hide();
            }
        }

        // ================================================================
        // MÉTODOS PRIVADOS
        // ================================================================

        /// <summary>
        /// Método para manejar la autenticación
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="UnauthorizedAccessException"></exception>
        private async Task<HttpResponseMessage> SendAuthenticatedRequestAsync(Func<Task<HttpResponseMessage>> request)
        {
            HttpResponseMessage response = await request();

            // ✅ Comprobar si la respuesta contiene un nuevo token
            if (response.Headers.TryGetValues("X-New-Token", out IEnumerable<string>? newTokenValues))
            {
                string? newToken = newTokenValues.FirstOrDefault();
                await _logService.LogInformationAsync($"✅ Nuevo token recibido: {newToken?.Length ?? 0}");
                if (!string.IsNullOrEmpty(newToken))
                {
                    // ✅ Actualizar el token en memoria y storage
                    await _authService.UpdateTokenAsync(newToken);
                    await _logService.LogInformationAsync("Token actualizado desde el servidor");
                }
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await _authService.LogoutAsync();

                _navigationManager.NavigateTo("/login", true);

                throw new UnauthorizedAccessException("Token expirado o inválido.");
            }

            return response;
        }

        private async Task<HttpResponseMessage> GetCall(string endpoint) => await SendAuthenticatedRequestAsync(() => _httpClient.GetAsync(endpoint));
        private async Task<HttpResponseMessage> PostCall<TRequest>(string endpoint, TRequest request) => await SendAuthenticatedRequestAsync(() => _httpClient.PostAsJsonAsync(endpoint, request, _jsonOptions));
        private async Task<HttpResponseMessage> PutCall<TRequest>(string endpoint, TRequest request) => await SendAuthenticatedRequestAsync(() => _httpClient.PutAsJsonAsync(endpoint, request, _jsonOptions));
        private async Task<HttpResponseMessage> DeleteCall(string endpoint) => await SendAuthenticatedRequestAsync(() => _httpClient.DeleteAsync(endpoint));
        private async Task<HttpResponseMessage> DeleteCall(HttpRequestMessage httpRequest) => await SendAuthenticatedRequestAsync(() => _httpClient.SendAsync(httpRequest));

        private async Task<ErrorResponse?> ParseErrorResponse(HttpResponseMessage response)
        {
            try
            {
                string content = await response.Content.ReadAsStringAsync();

                // 🔥 Si la respuesta está vacía, devolver un ErrorResponse por defecto
                if (string.IsNullOrWhiteSpace(content))
                {
                    return new ErrorResponse
                    {
                        StatusCode = (int)response.StatusCode,
                        Message = response.ReasonPhrase ?? "Error en la petición",
                        Timestamp = DateTime.UtcNow
                    };
                }

                if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                {
                    await _logService.LogInformationAsync($"API Response - Status: {response.StatusCode} - {content}");
                }
                else
                {
                    await _logService.LogInformationAsync($"API Error - Status: {response.StatusCode} - {content}");
                }

                return JsonSerializer.Deserialize<ErrorResponse>(content, _jsonOptions);
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"Error parsing error response", ex);

                return new ErrorResponse
                {
                    Message = response.ReasonPhrase ?? "Error en la petición",
                    StatusCode = (int)response.StatusCode
                };
            }
        }
    }
}