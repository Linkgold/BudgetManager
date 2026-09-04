using Microsoft.AspNetCore.Components;
using UI.Services;

namespace UI.Shared
{
    public partial class MainLayout : IDisposable
    {
        [Inject]
        private LoadingService LoadingService { get; set; } = default!;

        private bool _isLoading = false;

        protected override async Task OnInitializedAsync()
        {
            await ThemeService.LoadTheme();
            ThemeService.ThemeChanged += OnThemeChanged;
            LoadingService.OnLoadingChanged += OnLoadingChanged;
        }

        private void OnThemeChanged()
        {
            // 🔥 Forzar la actualización de la interfaz
            StateHasChanged();
        }

        private void OnLoadingChanged()
        {
            _isLoading = LoadingService.IsLoading;
            InvokeAsync(StateHasChanged);
        }

        public void Dispose()
        {
            ThemeService.ThemeChanged -= OnThemeChanged;
            LoadingService.OnLoadingChanged -= OnLoadingChanged;
        }
    }
}