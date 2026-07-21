namespace Shared.DTOs.Response
{
    /// <summary>
    /// DTO para devolver información de una categoría
    /// </summary>
    public class CategoryResponseDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}