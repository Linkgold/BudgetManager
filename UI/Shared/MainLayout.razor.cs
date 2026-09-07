using Microsoft.AspNetCore.Components;
using UI.Services;

namespace UI.Shared
{
    public partial class MainLayout : IDisposable
    {
        [Inject]
        private LoadingService LoadingService { get; set; } = default!;

        private bool _isLoading = false;
        private string _loadingMessage = "Cargando datos...";

        protected override async Task OnInitializedAsync()
        {
            await ThemeService.LoadTheme();
            ThemeService.ThemeChanged += OnThemeChanged;
            LoadingService.OnLoadingChanged += OnLoadingChanged;
            LoadingService.OnLoadingTextChanged += OnLoadingTextChanged;
        }

        private void OnThemeChanged()
        {
            StateHasChanged();
        }

        private void OnLoadingChanged()
        {
            _isLoading = LoadingService.IsLoading;
            _loadingMessage = LoadingService.CurrentMessage;
            InvokeAsync(StateHasChanged);
        }

        private void OnLoadingTextChanged(string message)
        {
            _loadingMessage = message;
            InvokeAsync(StateHasChanged);
        }

        public void Dispose()
        {
            ThemeService.ThemeChanged -= OnThemeChanged;
            LoadingService.OnLoadingChanged -= OnLoadingChanged;
            LoadingService.OnLoadingTextChanged -= OnLoadingTextChanged;
            LoadingService.Dispose();
        }
    }
}