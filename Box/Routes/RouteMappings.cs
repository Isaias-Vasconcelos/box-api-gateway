using Box.Entities;

namespace Box.Routes
{
    public static class RouteMappings
    {
        public static void MapServiceRoutes(this IEndpointRouteBuilder app, Gateway config)
        {
            foreach (var s in config.Services!)
            {
                var prefix = app.MapGroup($"/api/v{s.Value.Version}/{s.Key}");

                if (s.Value.Config!.EnableAuth) prefix.RequireAuthorization();

                if (config.Config!.UseRateLimit) prefix.RequireRateLimiting("rate_global");

                foreach (var e in s.Value.Endpoints!)
                {
                    switch (e.Value.Method?.ToUpper())
                    {
                        case "POST":
                            prefix.MapPost(e.Key, () => Results.Ok($"POST {e.Key}"));
                            break;
                        case "GET":
                            prefix.MapGet(e.Key, () => Results.Ok($"GET {e.Key}"));
                            break;
                        case "DELETE":
                            prefix.MapDelete(e.Key, () => Results.Ok($"DELETE {e.Key}"));
                            break;
                        case "PUT":
                            prefix.MapPut(e.Key, () => Results.Ok($"PUT {e.Key}"));
                            break;
                    }
                }
            }
        }

        public static void MapStaticRoutes(this IEndpointRouteBuilder app, Gateway config)
        {
            app.MapGet("/api/spec", () => Results.Ok(config));

            app.MapPost("/auth", () => Results.Ok("Auth endpoint"));
        }
    }
}