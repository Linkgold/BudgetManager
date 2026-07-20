namespace UI.Services.Interfaces
{
    public interface IToastService
    {
        event Action<string, string> OnShow;
        event Action OnClear;

        void ShowSuccess(string message);
        void ShowError(string message);
        void Show(string message, string type);
        void Clear();
    }
}