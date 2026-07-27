using Microsoft.AspNetCore.Components;
using Shared.DTOs.Request;
using Shared.DTOs.Response;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using UI.Services.Interfaces;

namespace UI.Services
{
    public class APIService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogService _logService;
        private readonly IAuthService _authService;
        private readonly NavigationManager _navigationManager;
        private readonly JsonSerializerOptions _jsonOptions;

        public APIService(HttpClient httpClient, IAuthService authService, NavigationManager navigationManager, ILogService logService)
        {
            _httpClient = httpClient;
            _authService = authService;
            _navigationManager = navigationManager;
            _logService = logService;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        // ================================================================
        // MÉTODOS GENÉRICOS CON AUTENTICACIÓN
        // ================================================================

        public async Task<T?> GetAsync<T>(string endpoint) where T : class
        {
            try
            {
                HttpResponseMessage response = await GetCall(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
                }

                await LogErrorAsync($"GET {endpoint}", response);

                return null;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"Error en GET {endpoint}", ex);

                return null;
            }
        }

        public async Task<List<T>?> GetListAsync<T>(string endpoint) where T : class
        {
            try
            {
                HttpResponseMessage response = await GetCall(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<T>>(_jsonOptions);
                }

                await LogErrorAsync($"GET {endpoint}", response);

                return null;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"Error en GET {endpoint}", ex);

                return null;
            }
        }

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest request)
            where TRequest : class
            where TResponse : class
        {
            try
            {
                HttpResponseMessage response = await PostCall(endpoint, request);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions);
                }

                await LogErrorAsync($"POST {endpoint}", response);
                return null;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"Error en POST {endpoint}", ex);

                return null;
            }
        }

        public async Task<bool> PostAsync<TRequest>(string endpoint, TRequest request) where TRequest : class
        {
            try
            {
                HttpResponseMessage response = await PostCall(endpoint, request);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                await LogErrorAsync($"POST {endpoint}", response);

                return false;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"Error en POST {endpoint}", ex);

                return false;
            }
        }

        public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest request)
            where TRequest : class
            where TResponse : class
        {
            try
            {
                HttpResponseMessage response = await PutCall(endpoint, request);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions);
                }

                await LogErrorAsync($"PUT {endpoint}", response);

                return null;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"Error en PUT {endpoint}", ex);

                return null;
            }
        }

        public async Task<bool> PutAsync<TRequest>(string endpoint, TRequest request) where TRequest : class
        {
            try
            {
                HttpResponseMessage response = await PutCall(endpoint, request);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                await LogErrorAsync($"PUT {endpoint}", response);

                return false;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"Error en PUT {endpoint}", ex);

                return false;
            }
        }

        public async Task<bool> DeleteAsync(string endpoint)
        {
            try
            {
                HttpResponseMessage response = await DeleteCall(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                await LogErrorAsync($"DELETE {endpoint}", response);

                return false;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"Error en DELETE {endpoint}", ex);

                return false;
            }
        }

        public async Task<T?> DeleteAsync<T>(string endpoint, object? request = null) where T : class
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
                        return await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
                    }

                    await LogErrorAsync($"DELETE {endpoint}", response);

                return null;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"Error en DELETE {endpoint}", ex);

                return null;
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

        private async Task LogErrorAsync(string operation, HttpResponseMessage response)
        {
            string content = await response.Content.ReadAsStringAsync();
            await _logService.LogErrorAsync($"API Error - {operation} - Status: {response.StatusCode}", new Exception(content));
        }
    }
}