namespace UI.Models.Forms
{
    public class FixedExpenseFormModel : FormModelBase
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int Month { get; set; } = DateTime.Now.Month;
        public int OriginalYear { get; set; }
        public int Year { get; set; } = DateTime.Now.Year;
    }
}