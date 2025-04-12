namespace Box.Services;

public interface IGatewayService
{
    Task<HttpResponseApi> GetServiceAsync(string origin , string endpoint);
    Task<HttpResponseApi> PostServiceAsync(string origin , string endpoint , Stream json);
    Task<HttpResponseApi> HandlePut();
    Task<HttpResponseApi> HandleDelete();
}