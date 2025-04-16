namespace Box.Http;

public interface IHttpService
{
    Task<HttpResponseMessage> Post(string url, HttpRequest request);
}