using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using UI.Services.Interfaces;

namespace UI.Shared
{
    /// <summary>
    /// Clase base para todas las páginas que necesiten logging
    /// </summary>
    public abstract class BasePage : ComponentBase
    {
        [Inject]
        protected ILogService LogService { get; set; } = default!;

        [Inject]
        protected NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        protected HttpClient Http { get; set; } = default!;

        [Inject]
        protected IJSRuntime JSRuntime { get; set; } = default!;

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

        protected void ShowErrorMessage(string message)
        {
            // Este método puede ser sobrescrito por las páginas hijas
            // para mostrar el mensaje en la UI
        }
    }
}
