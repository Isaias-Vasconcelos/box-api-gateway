namespace Box.Entities;

public class Auth
{
    public int StatusCode { get; set; }
    public string? Token { get; set; }
    public JsonElement Data { get; set; }
}