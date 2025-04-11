namespace Box.Entities
{
    public class Service
    {
        public Dictionary<string, Endpoint>? Endpoints { get; set; }
        public ServiceConfig? Config { get; set; }
    }
}
