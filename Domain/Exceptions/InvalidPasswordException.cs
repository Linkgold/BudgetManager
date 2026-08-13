/// <summary>
/// Excepción lanzada cuando el password no es válido
/// </summary>
public class InvalidPasswordException : Exception
{
    public InvalidPasswordException() { }

    public InvalidPasswordException(string message) : base(message) { }

    public InvalidPasswordException(string message, Exception innerException) : base(message, innerException) { }
}