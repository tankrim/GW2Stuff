namespace BarFoo.Infrastructure.Exceptions;

[Serializable]
internal class DuplicateApiKeyException : Exception
{
    public DuplicateApiKeyException()
    {
    }

    public DuplicateApiKeyException(string? message) : base(message)
    {
    }

    public DuplicateApiKeyException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}