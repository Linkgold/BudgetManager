using Contracts.Enums;
using UI.Extensions;
using UI.Models;

namespace UI.Extensions
{
    public static class TransactionExtensions
    {
        public static decimal GetDisplayAmount(this TransactionModel transaction)
        {
            return transaction.TransactionType switch
            {
                TransactionTypeEnum.Income => Math.Abs(transaction.Amount),
                TransactionTypeEnum.Expense => -Math.Abs(transaction.Amount),
                _ => Math.Abs(transaction.Amount)
            };
        }

        public static string GetFixedTypeLabel(this CategoryNatureEnum categoryNature)
        {
            if (categoryNature == CategoryNatureEnum.Mixed) return string.Empty;

            return categoryNature switch
            {
                CategoryNatureEnum.Income => "💰 Ingreso",
                CategoryNatureEnum.Expense => "💳 Gasto",
                _ => string.Empty
            };
        }

        public static TransactionTypeEnum GetDefaultTransactionTypeForCategory(this CategoryNatureEnum categoryNature)
        {
            return categoryNature switch
            {
                CategoryNatureEnum.Income => TransactionTypeEnum.Income,
                CategoryNatureEnum.Expense => TransactionTypeEnum.Expense,
                CategoryNatureEnum.Mixed => TransactionTypeEnum.Expense,
                _ => TransactionTypeEnum.Expense
            };
        }
       
        public static decimal GetTotalDisplayAmount(this IEnumerable<TransactionModel> transactions)
        {
            decimal total = 0m;

            foreach (TransactionModel item in transactions)
            {
                total += item.GetDisplayAmount();
            }

            return total;
        }

        public static int GetFirstMonthWithData(this IEnumerable<TransactionModel> transactions, int year)
        {
            List<int> monthsWithData = transactions
                .Where(t => t.Date.Year == year)
                .Select(t => t.Date.Month)
                .Distinct()
                .OrderBy(m => m)
                .ToList();

            return monthsWithData.FirstOrDefault();
        }

        public static bool HasMonthData(this IEnumerable<TransactionModel> transactions, int year, int month) => transactions.Any(t => t.Date.Year == year && t.Date.Month == month);
    }
}