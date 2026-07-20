namespace UI.Shared
{
    public partial class MainLayout : IDisposable
    {
        protected override async Task OnInitializedAsync()
        {
            await ThemeService.LoadTheme();
            ThemeService.ThemeChanged += OnThemeChanged;
        }

        private void OnThemeChanged()
        {
            // 🔥 Forzar la actualización de la interfaz
            StateHasChanged();
        }

        public void Dispose()
        {
            ThemeService.ThemeChanged -= OnThemeChanged;
        }
    }
}