using UI.Models.Forms;

namespace UI.Extensions
{
    public static class FormModeExtensions
    {
        public static string GetButtonClass(this FormModeEnum mode)
        {
            return mode switch
            {
                FormModeEnum.Create => "btn-primary",
                FormModeEnum.Edit => "btn-primary",
                FormModeEnum.Delete => "btn-danger",
                _ => "btn-primary"
            };
        }

        public static string GetButtonText(this FormModeEnum mode)
        {
            return mode switch
            {
                FormModeEnum.Create => "Guardar",
                FormModeEnum.Edit => "Guardar",
                FormModeEnum.Delete => "Eliminar",
                _ => "Guardar"
            };
        }

        public static string GetModalTitle(this FormModeEnum mode, string entityName)
        {
            return mode switch
            {
                FormModeEnum.Create => $"➕ Añadir {entityName}",
                FormModeEnum.Edit => $"✏️ Editar {entityName}",
                FormModeEnum.Delete => $"🗑️ Eliminar {entityName}",
                _ => entityName
            };
        }

        public static string GetModalWrapperClass(this FormModeEnum mode, bool isModalOpen) => !isModalOpen ? string.Empty : mode == FormModeEnum.Delete ? "modal-open-delete" : "modal-open";
    }
}