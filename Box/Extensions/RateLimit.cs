namespace Box.Extensions;

public static class RateLimit
{
    public static void AddRateLimit(this IServiceCollection services, Global config)
    {
        var rateLimitConfig = config.RateLimit;
        
        services.AddRateLimiter(l =>
        {
            foreach (var rule in rateLimitConfig)
            {
                l.AddFixedWindowLimiter(policyName: rule.Key, options =>
                    {
                        options.PermitLimit = rule.Value!.PermitLimit;
                        options.Window = rule.Value!.WindowTypeTime switch
                        {
                            "MILLISECONDS" => TimeSpan.FromMilliseconds(rule.Value.WindowTime),
                            "SECONDS" => TimeSpan.FromSeconds(rule.Value.WindowTime),
                            "MINUTES" => TimeSpan.FromMinutes(rule.Value.WindowTime),
                            "HOURS" => TimeSpan.FromHours(rule.Value.WindowTime),
                            _ => throw new InvalidOperationException("WindowTypeTime invalid!")
                        };

                        options.QueueProcessingOrder = rule.Value.QueueProcessingOrder switch
                        {
                            "OLDEST_FIRST" => QueueProcessingOrder.OldestFirst,
                            "NEWEST_FIRST" => QueueProcessingOrder.NewestFirst,
                            _ => throw new InvalidOperationException("QueueProcessingOrder invalid!")
                        };

                        options.QueueLimit = rule.Value!.QueueLimit;
                    })
                    .OnRejected = async (ctx, token) =>
                {
                    ctx.HttpContext.Response.StatusCode = 429;

                    if(ctx.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        await ctx.HttpContext.Response.WriteAsync($"Too many requests sent, please try again after {retryAfter.TotalMinutes} minute(s).", token);
                    }
                    else
                    {
                        await ctx.HttpContext.Response.WriteAsync($"Too many requests sent, please try again", token);
                    }
                };   
            }
        });
    }
}