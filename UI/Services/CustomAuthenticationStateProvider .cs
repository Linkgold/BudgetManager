using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace UI.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly HttpClient _httpClient;

        private const string TOKEN_KEY = "auth_token";
        private const string USER_NAME_KEY = "user_name";
        private const string USER_EMAIL_KEY = "user_email";

        public CustomAuthenticationStateProvider(IJSRuntime jsRuntime, HttpClient httpClient)
        {
            _jsRuntime = jsRuntime;
            _httpClient = httpClient;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // 🔥 Obtener el token del localStorage
            string? token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", TOKEN_KEY);
            ClaimsIdentity identity;
            ClaimsPrincipal principal;


            if (string.IsNullOrEmpty(token))
            {
                // 🔥 Usuario no autenticado
                identity = new ClaimsIdentity();
                principal = new ClaimsPrincipal(identity);
                return new AuthenticationState(principal);
            }

            // 🔥 Configurar el HttpClient para enviar el token
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 🔥 Crear claims del usuario
            string? userName = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", USER_NAME_KEY);
            string? email = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", USER_EMAIL_KEY);

            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"), // TODO: Obtener el ID real del token
                new Claim(ClaimTypes.Name, userName ?? string.Empty),
                new Claim(ClaimTypes.Email, email ?? string.Empty)
            };

            identity = new ClaimsIdentity(claims, "jwt");
            principal = new ClaimsPrincipal(identity);

            return new AuthenticationState(principal);
        }

        public void NotifyUserAuthentication(string token, string userName, string email)
        {
            // 🔥 Notificar que el usuario ha iniciado sesión
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.Email, email)
            };

            ClaimsIdentity identity = new ClaimsIdentity(claims, "jwt");
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
        }

        public void NotifyUserLogout()
        {
            // 🔥 Notificar que el usuario ha cerrado sesión
            ClaimsIdentity identity = new ClaimsIdentity();
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);

            _httpClient.DefaultRequestHeaders.Authorization = null;

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
        }
    }
}