using System.Threading.RateLimiting;
using Box.Entities;
using Microsoft.AspNetCore.RateLimiting;

namespace Box.Config;

public static class RateLimit
{
    public static void AddRateLimit(this IServiceCollection services, Gateway config)
    {
        if (!config.Config!.UseRateLimit) return;
        var rateLimitConfig = config.Config!.RateLimit;

        services.AddRateLimiter(_ =>
        {
            _.AddFixedWindowLimiter(policyName: "rate_global", options =>
                {
                    options.PermitLimit = rateLimitConfig!.PermitLimit;
                    options.Window = rateLimitConfig!.WindowTypeTime switch
                    {
                        "MILLISECONDS" => TimeSpan.FromMilliseconds(rateLimitConfig.WindowTime),
                        "SECONDS" => TimeSpan.FromSeconds(rateLimitConfig.WindowTime),
                        "MINUTES" => TimeSpan.FromMinutes(rateLimitConfig.WindowTime),
                        "HOURS" => TimeSpan.FromHours(rateLimitConfig.WindowTime),
                        _ => throw new InvalidOperationException("WindowTypeTime invalid!")
                    };

                    options.QueueProcessingOrder = rateLimitConfig.QueueProcessingOrder switch
                    {
                        "OLDEST_FIRST" => QueueProcessingOrder.OldestFirst,
                        "NEWEST_FIRST" => QueueProcessingOrder.NewestFirst,
                        _ => throw new InvalidOperationException("QueueProcessingOrder invalid!")
                    };

                    options.QueueLimit = rateLimitConfig!.QueueLimit;
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
        });
    }
}