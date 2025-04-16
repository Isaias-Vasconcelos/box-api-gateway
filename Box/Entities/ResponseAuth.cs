namespace Box.Entities;

public class ResponseAuth
{
    public int StatusCode { get; set; }
    public string? Token { get; set; }
    public JsonElement Data { get; set; }
}