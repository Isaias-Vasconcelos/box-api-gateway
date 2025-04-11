using Box.Response;

namespace Box.Services
{
    public interface IHttpService
    {
        Task<HttpResponseAPI> HandleGet(string origin , string endpoint);
        Task<HttpResponseAPI> HandlePost(string origin , string endpoit , Stream json);
        Task<HttpResponseAPI> HandlePut();
        Task<HttpResponseAPI> HandleDelete();
    }
}