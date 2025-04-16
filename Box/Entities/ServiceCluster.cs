namespace Box.Entities;

public class ServiceCluster
{
    public string? LoadBalancingPolicy { get; set; }
    public Dictionary<string, Origin>? Destinations { get; set; }
}