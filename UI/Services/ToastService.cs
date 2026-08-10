using UI.Models.Enum;
using UI.Services.Interfaces;

namespace UI.Services
{
    public class ToastService : IToastService
    {
        public event Action<string, ToastTypeEnum>? OnShow;
        public event Action? OnClear;

        public void ShowSuccess(string message) => Show(message, ToastTypeEnum.Success);
        public void ShowWarning(string message) => Show(message, ToastTypeEnum.Warning);
        public void ShowError(string message) => Show(message, ToastTypeEnum.Error);
        public void Show(string message, ToastTypeEnum type) => OnShow?.Invoke(message, type);
        public void Clear() => OnClear?.Invoke();
    }
}