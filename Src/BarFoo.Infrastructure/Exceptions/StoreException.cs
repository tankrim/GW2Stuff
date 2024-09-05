namespace BarFoo.Infrastructure.Exceptions;

[Serializable]
internal class StoreException : Exception
{
    public StoreException()
    {
    }

    public StoreException(string? message) : base(message)
    {
    }

    public StoreException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}