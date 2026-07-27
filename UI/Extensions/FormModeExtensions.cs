using UI.Models;

namespace UI.Extensions
{
    public static class FormModeExtensions
    {
        public static string GetButtonClass(this FormMode mode)
        {
            return mode switch
            {
                FormMode.Create => "btn-primary",
                FormMode.Edit => "btn-primary",
                FormMode.Delete => "btn-danger",
                _ => "btn-primary"
            };
        }

        public static string GetButtonText(this FormMode mode)
        {
            return mode switch
            {
                FormMode.Create => "Guardar",
                FormMode.Edit => "Guardar",
                FormMode.Delete => "Eliminar",
                _ => "Guardar"
            };
        }

        public static string GetModalTitle(this FormMode mode, string entityName)
        {
            return mode switch
            {
                FormMode.Create => $"➕ Nueva {entityName}",
                FormMode.Edit => $"✏️ Editar {entityName}",
                FormMode.Delete => $"🗑️ Eliminar {entityName}",
                _ => entityName
            };
        }
    }
}