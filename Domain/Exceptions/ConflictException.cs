namespace Domain.Exceptions
{
    /// <summary>
    /// Excepción lanzada cuando hay un conflicto de negocio (ej. duplicados)
    /// </summary>
    public class ConflictException : Exception
    {
        public ConflictException() { }

        public ConflictException(string message) : base(message) { }

        public ConflictException(string message, Exception innerException) : base(message, innerException) { }
    }
}