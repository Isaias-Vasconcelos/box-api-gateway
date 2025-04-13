namespace Box.Http
{
    public interface IHttpService
    {
        Task<HttpResponseMessage> GetAsync(string origin , string endpoint);
        Task<HttpResponseMessage> PostAsync(string origin , string endpoint , Stream json);
        Task<HttpResponseMessage> PutAsync();
        Task<HttpResponseMessage> DeleteAsync();
    }
}