namespace UI.Services.Interfaces
{
    public interface IThemeService
    {
        bool IsDark { get; }
        event Action? ThemeChanged;
        Task ToggleTheme();
        Task LoadTheme();
    }
}