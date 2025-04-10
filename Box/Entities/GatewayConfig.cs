namespace Box.Entities
{
    public class GatewayConfig
    {
        public bool UseCache { get; set; } = false;  
        public Cache? Cache { get; set; }
        public bool UseRateLimit { get; set; } = false;
        public RateLimit? RateLimit { get; set; }
    }
}
