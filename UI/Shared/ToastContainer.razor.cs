using Microsoft.AspNetCore.Components;

namespace UI.Shared
{
    public partial class ToastContainer
    {
        private string _message = string.Empty;
        private string _type = "success";

        protected override void OnInitialized()
        {
            ToastService.OnShow += ShowToast;
            ToastService.OnClear += ClearToast;
        }

        private void ShowToast(string message, string type)
        {
            _message = message;
            _type = type;
            StateHasChanged();

            // Auto-cerrar después de 4 segundos
            _ = Task.Delay(4000).ContinueWith(_ => ClearToast());
        }

        private void ClearToast()
        {
            _message = string.Empty;
            StateHasChanged();
        }

        public void Dispose()
        {
            ToastService.OnShow -= ShowToast;
            ToastService.OnClear -= ClearToast;
        }
    }
}