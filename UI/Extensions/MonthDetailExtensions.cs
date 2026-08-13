using Contracts.Enums;
using UI.Models.MonthDetail;

namespace UI.Extensions
{
    public static class MonthDetailExtensions
    {
        public static string GetExpandIcon(this Dictionary<string, bool> expandedCategories, string categoryName) => expandedCategories == null ? "▶" : expandedCategories.TryGetValue(categoryName, out bool value) && value ? "▼" : "▶";

        public static string GetDisplayByTransactionTypeClass(this TransactionTypeEnum typeEnum) => typeEnum == TransactionTypeEnum.Income ? "text-success" : "text-danger";
        
        public static string FormatCurrencyWithSign(this MonthDetailTransactionModel item) => $"{(item.TransactionType == TransactionTypeEnum.Income ? "+" : "-")}{item.DisplayAmount.FormatCurrency()}";

        public static string GetCardTypeClass(this decimal amount) => amount >= 0 ? "card-success" : "card-danger";
        
        public static string GetExtendedCardTypeClass(this decimal amount) => amount >= 0 ? "income card-success" : "expense card-danger";

        public static string FormatPercentage(this decimal amount) => amount.ToString("F1") + " %";
    }
}
