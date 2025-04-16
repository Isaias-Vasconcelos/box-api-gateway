namespace Box.Entities;

public class ServiceRoute
{
    public string? ClusterId { get; set; }
    public string? AuthorizationPolicy { get; set; }
    public string? RateLimiterPolicy { get; set; }
    public Match? Match { get; set; }
}