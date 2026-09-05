using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using UI.Services.Interfaces;

namespace UI.Pages
{
    public partial class Home : ComponentBase
    {
        [Inject]
        private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

        [Inject]
        private IThemeService ThemeService { get; set; } = default!;

        private bool _isAuthenticated = false;

        protected override async Task OnInitializedAsync()
        {
            AuthenticationState authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            _isAuthenticated = authState.User.Identity?.IsAuthenticated ?? false;
        }

        private async Task ToggleTheme()
        {
            await ThemeService.ToggleTheme();
        }
    }
}