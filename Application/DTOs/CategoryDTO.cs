namespace Application.DTOs
{
    public class CategoryDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateCategoryDTO
    {
        public string Name { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateCategoryDTO
    {
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}
