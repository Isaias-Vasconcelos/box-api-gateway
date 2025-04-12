namespace Box.Http.Implementations
{
    public class HttpService(HttpClient httpClient) : IHttpService
    {
        public Task<HttpResponseMessage> HandleDelete()
        {
            throw new NotImplementedException();
        }

        public async Task<HttpResponseMessage> HandleGet(string origin, string endpoint)
        {
            httpClient.BaseAddress = new Uri(origin);
            var response = await httpClient.GetAsync(endpoint);
            return response;
        }

        public async Task<HttpResponseMessage> HandlePost(string origin, string endpoint, Stream body)
        {
            httpClient.BaseAddress = new Uri(origin);

            using var reader = new StreamReader(body);
            var bodyString = await reader.ReadToEndAsync();

            var content = new StringContent(bodyString, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(endpoint, content);
            return response;
        }

        public Task<HttpResponseMessage> HandlePut()
        {
            throw new NotImplementedException();
        }
    }
}