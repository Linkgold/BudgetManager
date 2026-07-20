namespace UI.Shared
{
    public partial class NavMenu
    {
        private bool _collapseNavMenu = true;

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