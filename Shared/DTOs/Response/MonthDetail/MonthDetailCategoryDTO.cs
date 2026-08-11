using Contracts.Enums;

namespace Shared.DTOs.Response.MonthDetail
{
    public class MonthDetailCategoryDTO
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public CategoryNatureEnum Nature { get; set; }
        public decimal Budget { get; set; }
        public decimal Spent { get; set; }
        public List<MonthDetailTransactionDTO> Transactions { get; set; } = new();
    }
}