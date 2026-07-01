namespace Application.DTOs.Request
{
    /// <summary>
    /// DTO para crear una nueva categoría
    /// </summary>
    public class CreateCategoryRequestDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}