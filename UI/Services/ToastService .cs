using UI.Services.Interfaces;

namespace UI.Services
{
    public class ToastService : IToastService
    {
        public event Action<string, string>? OnShow;
        public event Action? OnClear;

        public void ShowSuccess(string message) => Show(message, "success");
        public void ShowError(string message) => Show(message, "error");
        public void Show(string message, string type) => OnShow?.Invoke(message, type);
        public void Clear() => OnClear?.Invoke();
    }
}