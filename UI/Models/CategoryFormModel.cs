using Contracts.Enums;

namespace UI.Models
{
    public class CategoryFormModel : FormModelBase
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public CategoryNatureEnum Nature { get; set; } = CategoryNatureEnum.Expense;
    }
}