using UI.Models;

namespace UI.Helpers
{
    public static class MonthHelper
    {
        private static List<MonthModel>? _months;

        public static IReadOnlyList<MonthModel> Months
        {
            get
            {
                if (_months == null)
                {
                    _months = new List<MonthModel>
                    {
                        new() { Value = 1, Name = "Enero", ShortName = "Ene" },
                        new() { Value = 2, Name = "Febrero", ShortName = "Feb" },
                        new() { Value = 3, Name = "Marzo", ShortName = "Mar" },
                        new() { Value = 4, Name = "Abril", ShortName = "Abr" },
                        new() { Value = 5, Name = "Mayo", ShortName = "May" },
                        new() { Value = 6, Name = "Junio", ShortName = "Jun" },
                        new() { Value = 7, Name = "Julio", ShortName = "Jul" },
                        new() { Value = 8, Name = "Agosto", ShortName = "Ago" },
                        new() { Value = 9, Name = "Septiembre", ShortName = "Sep" },
                        new() { Value = 10, Name = "Octubre", ShortName = "Oct" },
                        new() { Value = 11, Name = "Noviembre", ShortName = "Nov" },
                        new() { Value = 12, Name = "Diciembre", ShortName = "Dic" }
                    };
                }

                return _months;
            }
        }

        public static string GetMonthName(int month) => Months.FirstOrDefault(m => m.Value == month)?.Name ?? string.Empty;

        public static string GetMonthShortName(int month) => Months.FirstOrDefault(m => m.Value == month)?.ShortName ?? string.Empty;

        public static int GetMonthValue(string monthName) => Months.FirstOrDefault(m => m.Name == monthName)?.Value ?? 0;

        public static List<MonthModel> GetMonthsWithShortName() => Months.ToList();

        public static DateTime GetDefaultDate(int month, int year)
        {
            if (month == DateTime.Now.Month && year == DateTime.Now.Year)
            {
                return DateTime.Now;
            }

            // ✅ Usar el último día del mes seleccionado
            int lastDay = DateTime.DaysInMonth(year, month);
            return new DateTime(year, month, lastDay);
        }
    }
}