using Box.Response;
using System.Text;
using System.Text.Json;

namespace Box.Services.Implementations
{
    public class HttpService(HttpClient httpClient) : IHttpService
    {
        private readonly HttpClient _httpClient = httpClient;

        public Task<HttpResponseAPI> HandleDelete()
        {
            throw new NotImplementedException();
        }

        public async Task<HttpResponseAPI> HandleGet(string origin, string endpoint)
        {
            _httpClient.BaseAddress = new Uri(origin);

            var request = await _httpClient.GetAsync(endpoint);
            var data = await request.Content.ReadAsStringAsync();

            var json = JsonSerializer.Deserialize<JsonElement>(data);

            return Helper.Response(origin, endpoint, "GET", (int)request.StatusCode, json);
        }

        public async Task<HttpResponseAPI> HandlePost(string origin, string endpoint, Stream body)
        {
            _httpClient.BaseAddress = new Uri(origin);

            using var reader = new StreamReader(body);
            var bodyString = await reader.ReadToEndAsync();

            var content = new StringContent(bodyString, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(endpoint, content);
            var responseData = await response.Content.ReadAsStringAsync();

            var json = JsonSerializer.Deserialize<JsonElement>(responseData);

            return Helper.Response(origin, endpoint, "POST", (int)response.StatusCode, json);
        }

        public Task<HttpResponseAPI> HandlePut()
        {
            throw new NotImplementedException();
        }
    }
}