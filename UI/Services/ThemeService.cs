using UI.Services.Interfaces;

namespace UI.Services
{
    public class ThemeService : IThemeService
    {
        private readonly IStorageService _storageService;
        private bool _isDark = true; // Oscuro por defecto

        public event Action? ThemeChanged;

        public bool IsDark => _isDark;

        public ThemeService(IStorageService storageService)
        {
            _storageService = storageService;
        }

        public async Task LoadTheme()
        {
            string? stored = await _storageService.GetItemAsync("theme");

            if (!string.IsNullOrEmpty(stored))
            {
                _isDark = stored == "dark";
            }
            else
            {
                // Por defecto oscuro, lo guardamos
                await _storageService.SetItemAsync("theme", "dark");

                _isDark = true;
            }
            ThemeChanged?.Invoke();
        }

        public async Task ToggleTheme()
        {
            _isDark = !_isDark;
            await _storageService.SetItemAsync("theme", _isDark ? "dark" : "light");
            ThemeChanged?.Invoke();
        }
    }
}
