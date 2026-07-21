namespace Shared.DTOs.Request
{
    public class DeleteBulkBudgetRequestDTO: BulkBudgetRequestDTOBase
    {
        public List<int> MonthsToDelete { get; set; } = new List<int>(); // Meses a eliminar (1-12)
    }
}