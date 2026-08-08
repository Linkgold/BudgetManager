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
        private readonly ILogService _logService;
        private readonly IAuthService _authService;
        private readonly NavigationManager _navigationManager;
        private readonly IToastService _toastService;
        private readonly JsonSerializerOptions _jsonOptions;

        public APIService
        (
            HttpClient httpClient, 
            IAuthService authService, 
            NavigationManager navigationManager, 
            ILogService logService,
            IToastService toastService
        )
        {
            _httpClient = httpClient;
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

        public async Task<APIResult<T>> GetAsync<T>(string endpoint) where T : class
        {
            try
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
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"Error en GET {endpoint}", ex);

                return APIResult<T>.Failure(500, "Ocurrió un error inesperado.");
            }
        }
        public async Task<APIResult<List<T>>> GetListAsync<T>(string endpoint) where T : class
        {
            try
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
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"Error en GET {endpoint}", ex);

                return APIResult<List<T>>.Failure(500, "Ocurrió un error inesperado.");
            }
        }

        public async Task<APIResult<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest request)
            where TRequest : class
            where TResponse : class
        {
            try
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
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"Error en POST {endpoint}", ex);

                return APIResult<TResponse>.Failure(500, "Ocurrió un error inesperado.");
            }
        }

        public async Task<APIResult<TResponse>> PutAsync<TRequest, TResponse>(string endpoint, TRequest request)
            where TRequest : class
            where TResponse : class
        {
            try
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
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"Error en PUT {endpoint}", ex);

                return APIResult<TResponse>.Failure(500, "Ocurrió un error inesperado.");
            }
        }

        public async Task<APIResult<bool>> DeleteAsync(string endpoint)
        {
            try
            {
                HttpResponseMessage response = await DeleteCall(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    return APIResult<bool>.Success(true);
                }

                ErrorResponse? error = await ParseErrorResponse(response);

                return APIResult<bool>.Failure((int)response.StatusCode, error?.Message);
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"Error en DELETE {endpoint}", ex);

                return APIResult<bool>.Failure(500, "Ocurrió un error inesperado.");
            }
        }

        public async Task<APIResult<T>> DeleteAsync<T>(string endpoint, object? request = null) where T : class
        {
            try
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
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"Error en DELETE {endpoint}", ex);

                return APIResult<T>.Failure(500, "Ocurrió un error inesperado.");
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
        private async Task<HttpResponseMessage> PostCall<TRequest>(string endpoint, TRequest request) where TRequest : class => await SendAuthenticatedRequestAsync(() => _httpClient.PostAsJsonAsync(endpoint, request, _jsonOptions));
        private async Task<HttpResponseMessage> PutCall<TRequest>(string endpoint, TRequest request) where TRequest : class => await SendAuthenticatedRequestAsync(() => _httpClient.PutAsJsonAsync(endpoint, request, _jsonOptions));
        private async Task<HttpResponseMessage> DeleteCall(string endpoint) => await SendAuthenticatedRequestAsync(() => _httpClient.DeleteAsync(endpoint));
        private async Task<HttpResponseMessage> DeleteCall(HttpRequestMessage httpRequest) => await SendAuthenticatedRequestAsync(() => _httpClient.SendAsync(httpRequest));

        private async Task<ErrorResponse?> ParseErrorResponse(HttpResponseMessage response)
        {
            try
            {
                string content = await response.Content.ReadAsStringAsync();


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
        public void NotifySuccess(string message)
        {
            _toastService.ShowSuccess(message);
        }

        public void NotifyError(string message)
        {
            _toastService.ShowError(message);
        }
    }
}