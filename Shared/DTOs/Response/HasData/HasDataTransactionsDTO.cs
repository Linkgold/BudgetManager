namespace Shared.DTOs.Response.HasData
{
    public class HasDataTransactionsDTO
    {
        public List<int> Years { get; set; } = new();
        public Dictionary<int, List<int>> MonthsByYear { get; set; } = new(); // Año -> Lista de meses
    }
}