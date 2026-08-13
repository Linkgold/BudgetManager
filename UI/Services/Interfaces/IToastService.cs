using UI.Models.Enum;

namespace UI.Services.Interfaces
{
    public interface IToastService
    {
        event Action<string, ToastTypeEnum> OnShow;
        event Action OnClear;

        void ShowSuccess(string message);
        void ShowWarning(string message);
        void ShowError(string message);
        void Show(string message, ToastTypeEnum type);
        void Clear();
    }
}