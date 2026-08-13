using Microsoft.AspNetCore.Components;

namespace UI.Shared
{
    public partial class InfoBanner
    {
        [Parameter]
        public string Title { get; set; } = string.Empty;

        [Parameter]
        public string Message { get; set; } = string.Empty;

        [Parameter]
        public string ButtonText { get; set; } = string.Empty;

        [Parameter]
        public string ButtonUrl { get; set; } = string.Empty;
    }
}