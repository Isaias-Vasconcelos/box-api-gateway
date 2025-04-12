namespace Box.Http
{
    public interface IHttpService
    {
        Task<HttpResponseMessage> HandleGet(string origin , string endpoint);
        Task<HttpResponseMessage> HandlePost(string origin , string endpoint , Stream json);
        Task<HttpResponseMessage> HandlePut();
        Task<HttpResponseMessage> HandleDelete();
    }
}