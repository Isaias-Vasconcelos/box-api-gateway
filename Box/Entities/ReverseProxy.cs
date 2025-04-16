namespace Box.Entities;

public class ReverseProxy
{
    public Dictionary<string,ServiceRoute>? Routes { get; set; }
    public Dictionary<string, ServiceCluster>? Clusters { get; set; }
}