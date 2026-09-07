using System.Globalization;

namespace UI.Extensions
{
    public static class SharedExtensions
    {
        // ================================================================
        // CONSTANTES LOCALES
        // ================================================================

        private const string CULTURE_ES = "es-ES";
        private const string CURRENCY_FORMAT = "C";
        private const string ACTIVE_CLASS = "active";

        // ================================================================
        // CLASES CSS POR CONDICIÓN
        // ================================================================

        public static string GetTextClass(this bool condition) => condition ? CssClasses.SUCCESS : CssClasses.DANGER;

        public static string GetDisplayClass(this decimal amount) => (amount >= 0).GetTextClass();

        // ================================================================
        // FORMATO DE MONEDA
        // ================================================================

        public static string FormatCurrency(this decimal amount) => amount.ToString(CURRENCY_FORMAT, new CultureInfo(CULTURE_ES));

        public static string FormatValidAmount(this decimal amount) => amount > 0 ? amount.FormatCurrency() : "—";

        // ================================================================
        // CLASE ACTIVA PARA ENUMS
        // ================================================================

        public static string GetActiveClass<T>(this T currentValue, T targetValue) where T : Enum => currentValue.Equals(targetValue) ? ACTIVE_CLASS : string.Empty;
    }
}