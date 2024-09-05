namespace BarFoo.Core.Interfaces;

public interface IHttpClientWrapper
{
    Task<HttpResponseMessage> GetAsync(string url);
}
