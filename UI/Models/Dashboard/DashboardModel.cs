namespace UI.Models.Dashboard
{
    public class DashboardModel
    {
        public List<DashboardCategoryRow> Categories { get; set; } = new();
        public List<DashboardMonthColumn> Months { get; set; } = new();
        public List<DashboardAccumulatedMonth> AccumulatedMonths { get; set; } = new();
    }
}