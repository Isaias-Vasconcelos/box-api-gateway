namespace Box.Entities
{
    public class RateLimit
    {
        public int PermitLimit { get; set; }
        public string? WindowTypeTime { get; set; }
        public int WindowTime { get; set; } 
        public string? QueueProcessingOrder { get; set; }
        public int QueueLimit { get; set; }
    }
}
