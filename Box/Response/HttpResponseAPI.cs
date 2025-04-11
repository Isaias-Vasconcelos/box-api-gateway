using System.Text.Json;

namespace Box.Response
{
    public class HttpResponseAPI
    {
        public string? Origin { get; set; }    
        public string? Endpoint { get; set; }
        public string? Method { get; set; }
        public int StatusCode { get; set; }
        public JsonElement? Data { get; set; } 
    }
}