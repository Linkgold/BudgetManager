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
    }
}