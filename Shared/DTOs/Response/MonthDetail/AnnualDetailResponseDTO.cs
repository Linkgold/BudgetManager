namespace Shared.DTOs.Response.MonthDetail
{
    public class AnnualDetailResponseDTO
    {
        public int Year { get; set; }
        public List<MonthDetailResponseDTO> Months { get; set; } = new();
    }
}