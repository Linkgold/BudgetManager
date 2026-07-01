namespace Application.DTOs.Request
{
    /// <summary>
    /// DTO para actualizar una categoría existente
    /// </summary>
    public class UpdateCategoryRequestDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}