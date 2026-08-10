using Microsoft.AspNetCore.Components;
using UI.Models.Enum;

namespace UI.Shared
{
    public partial class Toast
    {
        [Parameter]
        public string Message { get; set; } = string.Empty;

        [Parameter]
        public ToastTypeEnum Type { get; set; } = ToastTypeEnum.Success;

        [Parameter]
        public EventCallback OnClose { get; set; }

        private async Task Close()
        {
            Message = string.Empty;
            if (OnClose.HasDelegate)
            {
                await OnClose.InvokeAsync();
            }
        }

        private string GetToastClass()
        {
            return Type switch
            {
                ToastTypeEnum.Success => "toast-success",
                ToastTypeEnum.Error => "toast-error",
                ToastTypeEnum.Warning => "toast-warning",
                _ => "toast-success"
            };
        }

        private string GetToastIcon()
        {
            return Type switch
            {
                ToastTypeEnum.Success => "✅",
                ToastTypeEnum.Error => "❌",
                ToastTypeEnum.Warning => "⚠️",
                _ => "✅"
            };
        }
    }
}