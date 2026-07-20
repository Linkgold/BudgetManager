using Microsoft.AspNetCore.Components;

namespace UI.Shared
{
    public partial class Toast
    {
        [Parameter]
        public string Message { get; set; } = string.Empty;

        [Parameter]
        public string Type { get; set; } = "success"; // "success" o "error"

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
    }
}