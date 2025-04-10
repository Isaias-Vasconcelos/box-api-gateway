namespace Box.Entities
{
    public class Service
    {
        public string? Version { get; set; }
        public Dictionary<string, Endpoint>? Endpoints { get; set; }
        public ServiceConfig? Config { get; set; }
    }
}
