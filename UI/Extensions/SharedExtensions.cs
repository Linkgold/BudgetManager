using System.Globalization;

namespace UI.Extensions
{
    public static class SharedExtensions
    {
        public static string GetDisplayClass(this decimal amount) => amount >= 0 ? "text-success" : "text-danger";

        public static string FormatCurrency(this decimal amount) => amount.ToString("C", new CultureInfo("es-ES"));

        public static string FormatValidAmount(this decimal amount) => amount > 0 ? amount.FormatCurrency() : "—";
    }
}