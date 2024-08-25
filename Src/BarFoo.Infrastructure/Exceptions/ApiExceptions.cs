namespace BarFoo.Infrastructure.Exceptions;

[Serializable]
public class ApiClientException : Exception
{
    public ApiClientException(string message, Exception innerException) : base(message, innerException)
    {
    }
    public ApiClientException(string message) : base(message)
    {
    }
}

[Serializable]
internal class ApiConnectionException : ApiClientException
{
    public ApiConnectionException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

[Serializable]
internal class ApiResponseFormatException : ApiClientException
{
    public ApiResponseFormatException(string message, Exception innerException) : base(message, innerException)
    {
    }
    public ApiResponseFormatException(string message) : base(message)
    {
    }
}

[Serializable]
internal class ApiUnauthorizedException : ApiClientException
{
    public ApiUnauthorizedException(string message) : base(message, null)
    {
    }
}

[Serializable]
internal class ApiForbiddenException : ApiClientException
{
    public ApiForbiddenException(string message) : base(message, null)
    {
    }
}

[Serializable]
internal class ApiNotFoundException : ApiClientException
{
    public ApiNotFoundException(string message) : base(message, null)
    {
    }
}

[Serializable]
internal class ApiRateLimitException : ApiClientException
{
    public ApiRateLimitException(string message) : base(message, null)
    {
    }
}

[Serializable]
internal class ApiTimeoutException : ApiClientException
{
    public ApiTimeoutException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

[Serializable]
public class ApiServiceUnavailableException : ApiClientException
{
    public ApiServiceUnavailableException(string message) : base(message, null)
    {
    }
}