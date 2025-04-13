namespace Box.Entities;

public class HttpRetry
{
    public int DelayTime { get; set; }
    public string? DelayTypeTime { get; set; }
    public int MaxRetryAttempts { get; set; }
}