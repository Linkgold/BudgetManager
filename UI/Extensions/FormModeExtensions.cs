using UI.Models.Forms;

namespace UI.Extensions
{
    public static class FormModeExtensions
    {
        // ================================================================
        // CONSTANTES LOCALES
        // ================================================================

        private const string BUTTON_CLASS_PRIMARY = "btn-primary";
        private const string BUTTON_CLASS_DANGER = "btn-danger";

        private const string BUTTON_TEXT_SAVE = "Guardar";
        private const string BUTTON_TEXT_DELETE = "Eliminar";

        private const string MODAL_TITLE_CREATE = "➕ Añadir ";
        private const string MODAL_TITLE_EDIT = "✏️ Editar ";
        private const string MODAL_TITLE_DELETE = "🗑️ Eliminar ";

        private const string MODAL_WRAPPER_OPEN = "modal-open";
        private const string MODAL_WRAPPER_DELETE = "modal-open-delete";

        // ================================================================
        // MÉTODOS
        // ================================================================


        public static string GetButtonClass(this FormModeEnum mode)
        {
            return mode switch
            {
                FormModeEnum.Delete => BUTTON_CLASS_DANGER,
                _ => BUTTON_CLASS_PRIMARY
            };
        }

        public static string GetButtonText(this FormModeEnum mode)
        {
            return mode switch
            {
                FormModeEnum.Delete => BUTTON_TEXT_DELETE,
                _ => BUTTON_TEXT_SAVE
            };
        }

        public static string GetModalTitle(this FormModeEnum mode, string entityName)
        {
            return mode switch
            {
                FormModeEnum.Create => $"{MODAL_TITLE_CREATE}{entityName}",
                FormModeEnum.Edit => $"{MODAL_TITLE_EDIT}{entityName}",
                FormModeEnum.Delete => $"{MODAL_TITLE_DELETE}{entityName}",
                _ => entityName
            };
        }

        public static string GetModalWrapperClass(this FormModeEnum mode, bool isModalOpen)
        {
            if (!isModalOpen) return string.Empty;

            return mode == FormModeEnum.Delete ? MODAL_WRAPPER_DELETE : MODAL_WRAPPER_OPEN;
        }
    }
}