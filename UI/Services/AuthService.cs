using Microsoft.JSInterop;
using Shared.DTOs.Request;
using Shared.DTOs.Response;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace UI.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;

        private const string TOKEN_KEY = "auth_token";
        private const string USER_NAME_KEY = "user_name";
        private const string USER_EMAIL_KEY = "user_email";

        public AuthService(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        public bool IsAuthenticated
        {
            get
            {
                // TODO: Verificar si el token existe y no ha expirado
                // Por ahora, solo comprobamos si existe en memoria
                return !string.IsNullOrEmpty(_token);
            }
        }

        private string? _token = null;
        private string? _userName = null;
        private string? _email = null;

        public string? UserName => _userName;
        public string? Email => _email;

        public async Task<bool> LoginAsync(string email, string password)
        {
            try
            {
                LoginRequestDTO request = new LoginRequestDTO
                {
                    Email = email,
                    Password = password
                };

                HttpResponseMessage response = await _httpClient.PostAsJsonAsync("/api/user/login", request);

                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }

                LoginResponseDTO? result = await response.Content.ReadFromJsonAsync<LoginResponseDTO>();

                if (result == null || string.IsNullOrEmpty(result.Token))
                {
                    return false;
                }

                // 🔥 Guardar token y datos del usuario
                _token = result.Token;
                _userName = result.UserName;
                _email = result.Email;

                // 🔥 Guardar en localStorage
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TOKEN_KEY, _token);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", USER_NAME_KEY, _userName ?? string.Empty);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", USER_EMAIL_KEY, _email ?? string.Empty);

                // 🔥 Configurar el HttpClient para enviar el token en las siguientes peticiones
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            // 🔥 Limpiar memoria
            _token = null;
            _userName = null;
            _email = null;

            // 🔥 Limpiar localStorage
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TOKEN_KEY);
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", USER_NAME_KEY);
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", USER_EMAIL_KEY);

            // 🔥 Limpiar el header de autorización
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        public async Task<string?> GetTokenAsync()
        {
            if (!string.IsNullOrEmpty(_token))
            {
                return _token;
            }

            // 🔥 Intentar recuperar el token de localStorage
            string? token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", TOKEN_KEY);

            if (!string.IsNullOrEmpty(token))
            {
                _token = token;

                // 🔥 Recuperar también los datos del usuario
                string? userName = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", USER_NAME_KEY);
                string? email = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", USER_EMAIL_KEY);

                _userName = userName;
                _email = email;

                // 🔥 Configurar el header de autorización
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            }

            return _token;
        }
    }
}
