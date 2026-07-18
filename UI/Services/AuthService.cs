using Shared.DTOs.Request;
using Shared.DTOs.Response;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using UI.Services.Interfaces;

namespace UI.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IStorageService _storageService;
        private readonly CustomAuthenticationStateProvider _authStateProvider;
        private readonly ILogService _logService;

        private const string TOKEN_KEY = "auth_token";
        private const string USER_NAME_KEY = "user_name";
        private const string USER_EMAIL_KEY = "user_email";

        public AuthService(ILogService logService, HttpClient httpClient, IStorageService storageService, CustomAuthenticationStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _storageService = storageService;
            _authStateProvider = authStateProvider;
            _logService = logService;
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

        public async Task<bool> LoginAsync(LoginRequestDTO request)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync("/api/user/login", request);

                if (!response.IsSuccessStatusCode)
                {
                    //  Log del error completo (con status code y contenido)
                    string errorContent = await response.Content.ReadAsStringAsync();
                    await _logService.LogErrorAsync($"Login fallido para {request.Email}. Status: {response.StatusCode}", new Exception(errorContent));
                    
                    return false;
                }

                LoginResponseDTO? result = await response.Content.ReadFromJsonAsync<LoginResponseDTO>();

                if (result == null || string.IsNullOrEmpty(result.Token))
                {
                    await _logService.LogErrorAsync($"Login fallido para {request.Email}. Respuesta inválida.", new Exception("LoginResponseDTO es null o token vacío"));

                    return false;
                }

                // 🔥 Guardar token y datos del usuario
                _token = result.Token;
                _userName = result.UserName;
                _email = result.Email;

                // 🔥 Guardar en localStorage
                await _storageService.SetItemAsync(TOKEN_KEY, _token);
                await _storageService.SetItemAsync(USER_NAME_KEY, _userName ?? string.Empty);
                await _storageService.SetItemAsync(USER_EMAIL_KEY, _email ?? string.Empty);

                // 🔥 Configurar el HttpClient para enviar el token en las siguientes peticiones
                _authStateProvider.NotifyUserAuthentication(result.Token, result.UserName, result.Email);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> RegisterAsync(CreateUserRequestDTO request)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync("/api/user/register", request);

                if (!response.IsSuccessStatusCode)
                {
                    //  Log del error completo (con status code y contenido)
                    string errorContent = await response.Content.ReadAsStringAsync();
                    await _logService.LogErrorAsync($"Registro fallido para {request.Email}. Status: {response.StatusCode}", new Exception(errorContent));

                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"Excepción durante el registro para {request.Email}.", ex);

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
            await _storageService.RemoveItemAsync(TOKEN_KEY);
            await _storageService.RemoveItemAsync(USER_NAME_KEY);
            await _storageService.RemoveItemAsync(USER_EMAIL_KEY);

            _httpClient.DefaultRequestHeaders.Authorization = null;

            // 🔥 Limpiar el header de autorización
            _authStateProvider.NotifyUserLogout();
        }

        public async Task<string?> GetTokenAsync()
        {
            if (!string.IsNullOrEmpty(_token))
            {
                return _token;
            }

            // 🔥 Intentar recuperar el token de localStorage
            string? token = await _storageService.GetItemAsync(TOKEN_KEY);

            if (!string.IsNullOrEmpty(token))
            {
                _token = token;

                // 🔥 Recuperar también los datos del usuario
                string? userName = await _storageService.GetItemAsync(USER_NAME_KEY);
                string? email = await _storageService.GetItemAsync(USER_EMAIL_KEY);

                _userName = userName;
                _email = email;

                // 🔥 Configurar el header de autorización
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            }

            return _token;
        }
    }
}
