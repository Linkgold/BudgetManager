using UI.Extensions;

namespace UI.Models
{
    public abstract class FormModelBase
    {
        public int Id { get; set; }
        public bool IsEditing { get; set; }
        public bool IsDeleting { get; set; }
        public bool IsModalOpen { get; set; }

        public FormMode CurrentMode
        {
            get
            {
                if (IsDeleting) return FormMode.Delete;
                if (IsEditing) return FormMode.Edit;
                return FormMode.Create;
            }
        }

        public string SaveButtonClass => CurrentMode.GetButtonClass();
        public string SaveButtonText => CurrentMode.GetButtonText();
    }
}