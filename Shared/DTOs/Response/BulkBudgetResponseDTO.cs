namespace Shared.DTOs.Response
{
    public class BulkBudgetResponseDTO
    {
        public int CategoryId { get; set; }
        public int Year { get; set; }
        public List<int> CreatedIds { get; set; } = new List<int>();
        public int TotalCreated { get; set; }
    }
}
