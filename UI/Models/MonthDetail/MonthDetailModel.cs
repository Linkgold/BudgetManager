using UI.Helpers;

namespace UI.Models.MonthDetail
{
    public class MonthDetailModel
    {
        public int Month { get; set; }
        public List<MonthDetailCategoryModel> Categories { get; set; } = new();
        public decimal TotalBudget { get; set; }
        public decimal TotalSpent { get; set; }

        // Propiedades calculadas
        public decimal TotalDifference => Categories.Sum(c => c.Difference);
        public decimal PercentageUsed => TotalBudget > 0 ? (TotalSpent / TotalBudget) * 100 : 0;
        public string MonthName => MonthHelper.GetMonthName(Month);
    }
}