namespace Domain.Exceptions
{
    /// <summary>
    /// Excepción lanzada cuando una entidad tiene dependencias y no puede ser eliminada
    /// </summary>
    public class DependencyException : Exception
    {
        public DependencyException() { }

        public DependencyException(string message) : base(message) { }

        public DependencyException(string message, Exception innerException) : base(message, innerException) { }
    }
}