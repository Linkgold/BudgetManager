namespace Shared.DTOs.Request
{
    /// <summary>
    /// DTO para actualizar una categoría existente
    /// </summary>
    public class UpdateCategoryRequestDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool Available { get; set; } = false;
    }
}