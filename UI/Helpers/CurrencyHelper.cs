using System.Globalization;

namespace UI.Helpers
{
    public static class CurrencyHelper
    {
        private static readonly CultureInfo SpanishCulture = new("es-ES");

        public static string FormatCurrency(decimal amount)
        {
            return amount.ToString("F2", SpanishCulture) + " €";
        }

        public static string FormatCurrencyWithSign(decimal amount)
        {
            string sign = amount >= 0 ? "+" : "-";

            return sign + FormatCurrency(amount);
        }
    }
}