using Shared.DTOs.Response.MonthDetail;

namespace UI.Models.MonthDetail
{
    public class AnnualDetailModel
    {
        public int Year { get; set; }
        public List<MonthDetailModel> Months { get; set; } = new();

        public MonthDetailModel? GetMonth(int month) => Months.FirstOrDefault(m => m.Month == month);

        public static AnnualDetailModel FromDTO(AnnualDetailResponseDTO dto)
        {
            AnnualDetailModel model = new AnnualDetailModel
            {
                Year = dto.Year,
                Months = new List<MonthDetailModel>()
            };

            foreach (MonthDetailResponseDTO monthDto in dto.Months)
            {
                MonthDetailModel monthModel = new MonthDetailModel
                {
                    Month = monthDto.Month,
                    Categories = new List<MonthDetailCategoryModel>(),
                    TotalBudget = monthDto.TotalBudget,
                    TotalSpent = monthDto.TotalSpent
                };

                foreach (MonthDetailCategoryDTO categoryDto in monthDto.Categories)
                {
                    MonthDetailCategoryModel categoryModel = new MonthDetailCategoryModel
                    {
                        CategoryId = categoryDto.CategoryId,
                        CategoryName = categoryDto.CategoryName,
                        Nature = categoryDto.Nature,
                        Budget = categoryDto.Budget,
                        Spent = categoryDto.Spent,
                        Transactions = new List<MonthDetailTransactionModel>()
                    };

                    foreach (MonthDetailTransactionDTO transactionDto in categoryDto.Transactions)
                    {
                        categoryModel.Transactions.Add(new MonthDetailTransactionModel
                        {
                            Id = transactionDto.Id,
                            Name = transactionDto.Name,
                            Description = transactionDto.Description,
                            Amount = transactionDto.Amount,
                            Date = transactionDto.Date,
                            TransactionType = transactionDto.TransactionType
                        });
                    }

                    monthModel.Categories.Add(categoryModel);
                }

                model.Months.Add(monthModel);
            }

            return model;
        }
    }
}