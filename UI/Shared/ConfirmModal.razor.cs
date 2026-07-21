using Microsoft.AspNetCore.Components;

namespace UI.Shared
{
    public partial class ConfirmModal
    {
        [Parameter]
        public bool IsVisible { get; set; }

        [Parameter]
        public string Title { get; set; } = "Confirmar";

        [Parameter]
        public string Message { get; set; } = "¿Estás seguro?";

        [Parameter]
        public string ConfirmButtonText { get; set; } = "Eliminar";

        [Parameter]
        public EventCallback OnConfirm { get; set; }

        [Parameter]
        public EventCallback OnCancel { get; set; }

        private async Task Confirm()
        {
            if (OnConfirm.HasDelegate)
            {
                await OnConfirm.InvokeAsync();
            }
            IsVisible = false;
        }

        private async Task Close()
        {
            if (OnCancel.HasDelegate)
            {
                await OnCancel.InvokeAsync();
            }
            IsVisible = false;
        }
    }
}