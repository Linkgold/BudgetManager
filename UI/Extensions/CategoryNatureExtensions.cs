using Contracts.Enums;

namespace UI.Extensions
{
    public static class CategoryNatureExtensions
    {
        // ================================================================
        // CONSTANTES LOCALES
        // ================================================================

        private const string CIRCLE_COLOR_GREEN = "green";
        private const string CIRCLE_COLOR_YELLOW = "yellow";
        private const string CIRCLE_COLOR_RED = "red";
        public const string BADGE_INCOME = "bg-success";
        public const string BADGE_EXPENSE = "bg-danger";
        public const string BADGE_MIXED = "bg-warning text-dark";
        public const string BADGE_DEFAULT = "bg-secondary";

        // ================================================================
        // MÉTODOS
        // ================================================================

        private static readonly Dictionary<CategoryNatureEnum, int> _order = new()
        {
            { CategoryNatureEnum.Income, 0 },
            { CategoryNatureEnum.Mixed, 1 },
            { CategoryNatureEnum.Expense, 2 }
        };

        public static IOrderedEnumerable<T> OrderByNature<T>(this IEnumerable<T> source, Func<T, CategoryNatureEnum> natureSelector) => source.OrderBy(item => _order[natureSelector(item)]);

        public static IOrderedEnumerable<T> OrderByDateDescendingAndNature<T>
        (
            this IEnumerable<T> source,
            Func<T, DateTime> dateSelector,
            Func<T, CategoryNatureEnum> natureSelector
        )
        {
            return source
                .OrderByDescending(dateSelector)
                .ThenBy(item => _order[natureSelector(item)]);
        }

        public static IOrderedEnumerable<T> OrderByNatureWithIncomeBudget<T>
        (
            this IEnumerable<T> source,
            Func<T, CategoryNatureEnum> natureSelector,
            Func<T, decimal> budgetSelector // ✅ Recibe el budget directamente
        )
        {
            return source
                .OrderByNature(natureSelector)
                .ThenBy(item => natureSelector(item) == CategoryNatureEnum.Income
                    ? (budgetSelector(item) > 0 ? 0 : 1) : 2);
        }

        public static string GetBadgeClass(this CategoryNatureEnum nature)
        {
            return nature switch
            {
                CategoryNatureEnum.Income => BADGE_INCOME,
                CategoryNatureEnum.Expense => BADGE_EXPENSE,
                CategoryNatureEnum.Mixed => BADGE_MIXED,
                _ => BADGE_DEFAULT
            };
        }

        public static string GetIcon(this CategoryNatureEnum nature)
        {
            return nature switch
            {
                CategoryNatureEnum.Income => Icons.INCOME,
                CategoryNatureEnum.Expense => Icons.EXPENSE,
                CategoryNatureEnum.Mixed => Icons.MIXED,
                _ => Icons.UNKNOWN
            };
        }

        public static string GetDisplayName(this CategoryNatureEnum nature)
        {
            return nature switch
            {
                CategoryNatureEnum.Income => Labels.INCOME,
                CategoryNatureEnum.Expense => Labels.EXPENSE,
                CategoryNatureEnum.Mixed => Labels.MIXED,
                _ => Labels.UNKNOWN
            };
        }

        /// <summary>
        /// Obtiene la clase CSS para el color del estado del gasto según la naturaleza
        /// </summary>
        public static string GetPercentageStatusClass(this CategoryNatureEnum nature, decimal spent, decimal budget)
        {
            if (budget == 0 && spent == 0) return CssClasses.MUTED;

            if (budget == 0)
            {
                return (spent > 0).GetTextClass();
            }

            spent = Math.Abs(spent);
            budget = Math.Abs(budget);

            decimal percentage = (spent / budget) * 100;

            if (nature == CategoryNatureEnum.Income)
            {
                return percentage switch
                {
                    >= 100 => CssClasses.SUCCESS,
                    >= 80 => CssClasses.WARNING,
                    _ => CssClasses.DANGER
                };
            }
            else
            {
                if (percentage <= 100)
                {
                    return CssClasses.SUCCESS;
                }

                // Calcular desviación sobre el presupuesto
                decimal deviation = percentage - 100;

                // 🔥 UMBRAL DINÁMICO: cuanto más alto el presupuesto, menos tolerancia
                // Escala: presupuesto de 100€ → 40% de tolerancia, presupuesto de 1000€ → 10% de tolerancia
                decimal tolerance = Math.Max(10, 40 - (budget / 25)); // Ajusta la fórmula según tus necesidades

                // Si la desviación supera la tolerancia dinámica → rojo, sino naranja
                return deviation > tolerance ? CssClasses.DANGER : CssClasses.WARNING;
            }
        }

        public static string GetStatusCircleClass(this CategoryNatureEnum nature, decimal spent, decimal budget)
        {
            return nature.GetPercentageStatusClass(spent, budget) switch
            {
                CssClasses.WARNING => CIRCLE_COLOR_YELLOW,
                CssClasses.DANGER => CIRCLE_COLOR_RED,
                _ => CIRCLE_COLOR_GREEN
            };
        }

        public static string GetStatusClass(this CategoryNatureEnum nature) => (nature == CategoryNatureEnum.Income).GetTextClass();
    }
}