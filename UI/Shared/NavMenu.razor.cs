using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using UI.Services.Interfaces;

namespace UI.Shared
{
    public partial class NavMenu : IDisposable
    {
        [Inject]
        private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

        [Inject]
        private ILogService LogService { get; set; } = default!;

        private string _userName = string.Empty;
        private bool _collapseNavMenu = true;
        private bool _isAuthenticated = false;

        protected override async Task OnInitializedAsync()
        {
            // ✅ Obtener el nombre del usuario autenticado
            AuthenticationState authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            
            UpdateUserData(authState);

            AuthenticationStateProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;

            await base.OnInitializedAsync();
        }

        private async void OnAuthenticationStateChanged(Task<AuthenticationState> task)
        {
            try
            {
                AuthenticationState authState = await task;

                UpdateUserData(authState);

                StateHasChanged();
            }
            catch (Exception ex)
            {
                await LogService.LogErrorAsync("Error al actualizar el estado de autenticación en NavMenu", ex);
            }
        }

        private void UpdateUserData(AuthenticationState authState)
        {
            ClaimsPrincipal user = authState.User;

            _isAuthenticated = user.Identity?.IsAuthenticated ?? false;

            if (_isAuthenticated)
            {
                _userName = user.FindFirst(ClaimTypes.Name)?.Value ?? user.Identity?.Name ?? "Usuario";
            }
            else
            {
                _userName = string.Empty;
            }
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
            await AuthService.LogoutAsync();

            NavigationManager.NavigateTo("/", true);
        }

        private async Task ToggleTheme()
        {
            await ThemeService.ToggleTheme();
        }

        public void Dispose()
        {
            // 🔥 Desuscribirse para evitar memory leaks
            AuthenticationStateProvider.AuthenticationStateChanged -= OnAuthenticationStateChanged;
        }
    }
}