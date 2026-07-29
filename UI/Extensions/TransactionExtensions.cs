using Contracts.Enums;
using UI.Helpers;
using UI.Models;

namespace UI.Extensions
{
    public static class TransactionExtensions
    {
        public static decimal GetDisplayAmount(this TransactionModel transaction)
        {
            return transaction.CategoryNature switch
            {
                CategoryNatureEnum.Income => Math.Abs(transaction.Amount),
                CategoryNatureEnum.Expense => -Math.Abs(transaction.Amount),
                CategoryNatureEnum.Mixed => transaction.Amount > 0 ? Math.Abs(transaction.Amount) : -Math.Abs(transaction.Amount),
                _ => Math.Abs(transaction.Amount)
            };
        }

        public static string GetDisplayClass(this TransactionModel transaction)
        {
            decimal displayAmount = transaction.GetDisplayAmount();
            return displayAmount >= 0 ? "text-success" : "text-danger";
        }

        public static string GetFormattedDisplay(this TransactionModel transaction)
        {
            decimal displayAmount = transaction.GetDisplayAmount();
            string sign = displayAmount >= 0 ? "+" : "-";
            return $"{sign}{CurrencyHelper.FormatCurrency(Math.Abs(displayAmount))}";
        }

        // ✅ Extensión para colecciones
        public static decimal GetTotalDisplayAmount(this IEnumerable<TransactionModel> transactions)
        {
            decimal total = 0m;

            foreach (TransactionModel item in transactions)
            {
                total += item.GetDisplayAmount();
            }

            return total;
        }
    }
}