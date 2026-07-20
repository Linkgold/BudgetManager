using Microsoft.AspNetCore.Components;

namespace UI.Shared
{
    public partial class Modal
    {
        [Parameter]
        public string Title { get; set; } = string.Empty;

        [Parameter]
        public bool IsVisible { get; set; }

        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        [Parameter]
        public EventCallback OnSave { get; set; }

        [Parameter]
        public EventCallback OnClose { get; set; }

        [Parameter]
        public string SaveButtonText { get; set; } = "Guardar";

        [Parameter]
        public string SaveButtonClass { get; set; } = "btn-primary";

        [Parameter]
        public bool IsDeleteMode { get; set; } = false;

        private async Task CloseModal()
        {
            if (OnClose.HasDelegate)
            {
                await OnClose.InvokeAsync();
            }
            IsVisible = false;
        }
    }
}
