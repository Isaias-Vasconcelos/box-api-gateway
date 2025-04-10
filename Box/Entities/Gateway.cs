namespace Box.Entities
{
    public class Gateway
    {
        public string? Version { get; set; } = "1";
        public string? AppName { get; set; } = "GATEWAY_DEFAULT";
        public Dictionary<string, Service>? Services { get; set; }
        public AuthInfo? AuthInfo { get; set; }
        public GatewayConfig? Config { get; set; }
    }
}
