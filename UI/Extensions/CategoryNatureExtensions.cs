using Contracts.Enums;

namespace UI.Extensions
{
    public static class CategoryNatureExtensions
    {
        public static string GetBadgeClass(this CategoryNatureEnum nature)
        {
            return nature switch
            {
                CategoryNatureEnum.Income => "bg-success",
                CategoryNatureEnum.Expense => "bg-danger",
                CategoryNatureEnum.Mixed => "bg-warning text-dark",
                _ => "bg-secondary"
            };
        }

        public static string GetIcon(this CategoryNatureEnum nature)
        {
            return nature switch
            {
                CategoryNatureEnum.Income => "💰",
                CategoryNatureEnum.Expense => "💳",
                CategoryNatureEnum.Mixed => "🔄",
                _ => "❓"
            };
        }

        public static string GetDisplayName(this CategoryNatureEnum nature)
        {
            return nature switch
            {
                CategoryNatureEnum.Income => "Ingreso",
                CategoryNatureEnum.Expense => "Gasto",
                CategoryNatureEnum.Mixed => "Mixto",
                _ => "Desconocido"
            };
        }

        /// <summary>
        /// Obtiene la clase CSS para el color del estado del gasto según la naturaleza
        /// </summary>
        public static string GetPercentageStatusClass(this CategoryNatureEnum nature, decimal spent, decimal budget)
        {
            spent = Math.Abs(spent);
            budget = Math.Abs(budget);

            if (budget == 0) return "text-muted";

            decimal percentage = (spent / budget) * 100;

            if (nature == CategoryNatureEnum.Income)
            {
                if (percentage >= 100)
                {
                    return "text-success";
                }
                else if (percentage >= 80)
                {
                    return "text-warning";
                }
                else
                {
                    return "text-danger";
                }
            }
            else
            {
                if (percentage < 80)
                {
                    return "text-success";
                }
                else if (percentage <= 100)
                {
                    return "text-warning";
                }
                else
                {
                    return "text-danger";
                }
            }
        }

        public static string GetStatusCircleClass(this CategoryNatureEnum nature, decimal spent, decimal budget)
        {
            string textClass = nature.GetPercentageStatusClass(spent, budget);

            return textClass switch
            {
                "text-success" => "green",
                "text-warning" => "yellow",
                "text-danger" => "red",
                _ => "green"
            };
        }

        public static string GetStatusClass(this CategoryNatureEnum nature)
        {
            return nature switch
            {
                CategoryNatureEnum.Income => "text-success",
                _ => "text-danger"
            };
        }
    }
}