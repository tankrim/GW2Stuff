namespace BarFoo.Infrastructure.Services;

public interface IHttpClientWrapper
{
    Task<HttpResponseMessage> GetAsync(string url);
}
