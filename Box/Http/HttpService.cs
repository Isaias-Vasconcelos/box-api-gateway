namespace Box.Http;

public class HttpService(HttpClient http):IHttpService
{
    public async Task<HttpResponseMessage> Post(string url, HttpRequest request)
    {
        var body = request.Body;
        using var reader = new StreamReader(body);
        var bodyString = await reader.ReadToEndAsync();
        var content = new StringContent(bodyString, Encoding.UTF8, "application/json");
        var response = await http.PostAsync(url, content);
        return response;
    }
}