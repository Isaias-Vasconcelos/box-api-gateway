namespace Box.Entities;

public class Global
{
    public string? AuthOrigin { get; set; }
    public Dictionary<string, RateLimit>? RateLimit { get; set; }
    public ReverseProxy? ReverseProxy { get; set; }
}