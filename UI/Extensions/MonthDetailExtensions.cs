using Contracts.Enums;
using UI.Pages;

namespace UI.Extensions
{
    public static class MonthDetailExtensions
    {
        // ================================================================
        // CONSTANTES LOCALES
        // ================================================================

        private const string CARD_SUCCESS = "card-success";
        private const string CARD_DANGER = "card-danger";
        private const string CARD_INCOME = "income card-success";
        private const string CARD_EXPENSE = "expense card-danger";

        private const string VIEW_TITLE_CATEGORY = "📊 Presupuestos vs movimientos ejecutados por categoría";
        private const string VIEW_TITLE_DAY = "🗓️ Movimientos ejecutados por día";

        private const string PERCENTAGE_FORMAT = "F1";

        // ================================================================
        // MÉTODOS
        // ================================================================

        public static string GetDisplayByTransactionTypeClass(this TransactionTypeEnum typeEnum) => (typeEnum == TransactionTypeEnum.Income).GetTextClass();

        public static string GetCardTypeClass(this decimal amount) => amount >= 0 ? CARD_SUCCESS : CARD_DANGER;
        
        public static string GetExtendedCardTypeClass(this decimal amount) => amount >= 0 ? CARD_INCOME : CARD_EXPENSE;

        public static string FormatPercentage(this decimal amount) => amount.ToString(PERCENTAGE_FORMAT) + " %";

        internal static string GetViewModeTitle(this ViewModeEnum viewMode)
        {
            return viewMode == ViewModeEnum.Category
                ? VIEW_TITLE_CATEGORY
                : VIEW_TITLE_DAY;
        }

        private static string GetExpandIconInternal<TKey>(Dictionary<TKey, bool>? expandedItems, TKey key) where TKey : notnull
        {
            return expandedItems != null && expandedItems.TryGetValue(key, out bool value) && value
                ? Icons.EXPAND_OPEN
                : Icons.EXPAND_CLOSED;
        }

        public static string GetExpandIcon(this Dictionary<string, bool> expandedCategories, string categoryName) => GetExpandIconInternal(expandedCategories, categoryName);
        public static string GetDayExpandIcon(this Dictionary<int, bool> expandedDays, int day) => GetExpandIconInternal(expandedDays, day);
    }
}