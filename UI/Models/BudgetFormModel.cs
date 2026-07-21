namespace UI.Models
{
    public class BudgetFormModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int Year { get; set; }
        public decimal DefaultAmount { get; set; }
        public Dictionary<int, decimal> MonthlyAmounts { get; set; } = new Dictionary<int, decimal>();
        public bool IsEditing { get; set; }
        public bool IsDeleting { get; set; }
    }
}