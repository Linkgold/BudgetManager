using System.Globalization;

namespace UI.Extensions
{
    public static class SharedExtensions
    {
        private static readonly CultureInfo SpanishCulture = new("es-ES");

        public static string GetDisplayClass(this decimal amount) => amount >= 0 ? "text-success" : "text-danger";

        public static string FormatCurrency(this decimal amount) => amount.ToString("F2", SpanishCulture) + " €";

        public static string FormatCurrencyWithSign(this decimal amount) => $"{(amount >= 0 ? "+" : "-")}{FormatCurrency(amount)}";
    }
}