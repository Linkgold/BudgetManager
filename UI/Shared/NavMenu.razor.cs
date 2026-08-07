using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace UI.Shared
{
    public partial class NavMenu
    {
        [Inject]
        private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

        private string _userName = string.Empty;
        private bool _collapseNavMenu = true;

        protected override async Task OnInitializedAsync()
        {
            // ✅ Obtener el nombre del usuario autenticado
            AuthenticationState authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            ClaimsPrincipal user = authState.User;

            if (user.Identity?.IsAuthenticated == true)
            {
                _userName = user.FindFirst(ClaimTypes.Name)?.Value ?? user.Identity.Name ?? "Usuario";
            }

            await base.OnInitializedAsync();
        }

        private void ToggleNavMenu()
        {
            _collapseNavMenu = !_collapseNavMenu;
        }

        private void CloseMenu()
        {
            _collapseNavMenu = true;
        }

        private async Task Logout()
        {
            // 🔥 Forzar tema claro antes de cerrar sesión (para que login/register se vean bien)
            if (ThemeService.IsDark)
            {
                await ThemeService.ToggleTheme();   // Cambia a claro
            }

            await AuthService.LogoutAsync();
            NavigationManager.NavigateTo("/login", true);
        }

        private async Task ToggleTheme()
        {
            await ThemeService.ToggleTheme();
        }
    }
}