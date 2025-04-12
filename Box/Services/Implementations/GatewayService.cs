namespace Box.Services.Implementations
{
    public class GatewayService(IHttpService http) : IGatewayService
    {
        public Task<HttpResponseApi> HandleDelete()
        {
            throw new NotImplementedException();
        }

        public async Task<HttpResponseApi> GetServiceAsync(string origin, string endpoint)
        {
            var request = await http.HandleGet(origin, endpoint);
            var data = await request.Content.ReadAsStringAsync();
            var json = JsonSerializer.Deserialize<JsonElement>(data);

            return Helper.Response(origin, endpoint, "GET", (int)request.StatusCode, json);
        }

        public async Task<HttpResponseApi> PostServiceAsync(string origin, string endpoint, Stream body)
        {
            var response = await http.HandlePost(origin, endpoint, body);
            var responseData = await response.Content.ReadAsStringAsync();
            var json = JsonSerializer.Deserialize<JsonElement>(responseData);

            return Helper.Response(origin, endpoint, "POST", (int)response.StatusCode, json);
        }

        public Task<HttpResponseApi> HandlePut()
        {
            throw new NotImplementedException();
        }
    }
}