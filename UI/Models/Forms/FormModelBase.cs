using UI.Extensions;

namespace UI.Models.Forms
{
    public abstract class FormModelBase
    {
        public int Id { get; set; }
        public bool IsEditing { get; set; }
        public bool IsDeleting { get; set; }
        public bool IsModalOpen { get; set; }

        public FormModeEnum CurrentMode
        {
            get
            {
                if (IsDeleting) return FormModeEnum.Delete;
                if (IsEditing) return FormModeEnum.Edit;
                return FormModeEnum.Create;
            }
        }

        public string SaveButtonClass => CurrentMode.GetButtonClass();
        public string SaveButtonText => CurrentMode.GetButtonText();
    }
}