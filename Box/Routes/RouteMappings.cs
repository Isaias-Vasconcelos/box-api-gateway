namespace Box.Routes
{
    public static class RouteMappings
    {
        public static void MapServiceRoutes(this IEndpointRouteBuilder app)
        {
            var config = app.ServiceProvider.GetRequiredService<Gateway>();

            foreach (var s in config.Services!)
            {
                var prefix = app.MapGroup($"");
                
                if (config.Config!.UseRateLimit) prefix.RequireRateLimiting("rate_global");

                foreach (var e in s.Value.Endpoints!)
                {
                    switch (e.Value.Method?.ToUpper())
                    {
                        case "POST":
                            prefix.MapPost(e.Value.Path!,async (HttpRequest request , IGatewayService service) =>
                            {
                                var response = await service.PostServiceAsync(s.Value.Config.Origin!, e.Value.Path!, request.Body);
                                return Results.Ok(response);
                            });
                            break;
                        case "GET":
                            var path = ConvertUrl(e.Value.Path!);
                            prefix.MapGet(path,async (HttpRequest request, IGatewayService service) =>
                            {
                                var originalUrl = e.Value.Path!;
                                var fullUrl = GetFullUrl(request, originalUrl);
                                var response = await service.GetServiceAsync(s.Value.Config.Origin!, fullUrl);
                                return Results.Json(response, statusCode: response.StatusCode);
                            });
                            break;
                        case "DELETE":
                            prefix.MapDelete(e.Value.Path!, () => Results.Ok($"DELETE {e.Key}"));
                            break;
                        case "PUT":
                            prefix.MapPut(e.Value.Path!, () => Results.Ok($"PUT {e.Key}"));
                            break;
                    }
                }
            }
        }

        public static void MapStaticRoutes(this IEndpointRouteBuilder app)
        {
            var config = app.ServiceProvider.GetRequiredService<Gateway>();

            app.MapGet("/api/spec", () => Results.Ok(config));
            app.MapPost("/auth", async (HttpRequest request, IAuthService service) =>
            {
                var auth = await service.AuthenticateServiceAsync(config,request.Body);
            });
        }

        private static string ConvertUrl(string url)
        {
            return Regex.Replace(url, @"\[(\w+)\]", "{$1}");
        }

        private static string GetFullUrl(HttpRequest request, string url)
        {
            var resolvedPath = Regex.Replace(url, @"\[(\w+)\]", match =>
            {
                var key = match.Groups[1].Value;

                if (request.RouteValues.TryGetValue(key, out var value) && value != null)
                    return Uri.EscapeDataString(value.ToString()!);

                return match.Value;
            });

            var queryString = request.QueryString.HasValue ? request.QueryString.Value : "";
            var fullUrl = resolvedPath + queryString;

            return fullUrl;
        }
    }
}