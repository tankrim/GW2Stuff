namespace BarFoo.Core.Exceptions;

[Serializable]
internal class ApiKeyNotFoundException : Exception
{
    public ApiKeyNotFoundException()
    {
    }

    public ApiKeyNotFoundException(string? message) : base(message)
    {
    }

    public ApiKeyNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}