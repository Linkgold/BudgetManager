using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net;
using UI.Services;
using UI.Services.Interfaces;

namespace UI.Shared
{
    /// <summary>
    /// Clase base para todas las páginas que necesiten logging
    /// </summary>
    public abstract class BasePage : ComponentBase
    {
        [Inject]
        private IAuthService AuthService { get; set; } = default!;

        [Inject]
        protected ILogService LogService { get; set; } = default!;

        [Inject]
        protected NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        protected HttpClient Http { get; set; } = default!;

        protected async Task LogErrorAsync(string message, Exception? exception = null)
        {
            await LogService.LogErrorAsync(message, exception);
        }

        protected async Task LogWarningAsync(string message)
        {
            await LogService.LogWarningAsync(message);
        }

        protected async Task LogInfoAsync(string message)
        {
            await LogService.LogInfoAsync(message);
        }

        protected async Task<HttpResponseMessage> SendAuthenticatedRequestAsync(Func<Task<HttpResponseMessage>> request)
        {
            HttpResponseMessage response = await request();

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await AuthService.LogoutAsync();
                NavigationManager.NavigateTo("/login", true);
                throw new UnauthorizedAccessException("Token expirado o inválido.");
            }

            return response;
        }
    }
}
