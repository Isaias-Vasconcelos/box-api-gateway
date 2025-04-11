using Box.Entities;
using Box.Services;
using System.Text.RegularExpressions;

namespace Box.Routes
{
    public static class RouteMappings
    {
        public static void MapServiceRoutes(this IEndpointRouteBuilder app, Gateway config)
        {
            foreach (var s in config.Services!)
            {
                var prefix = app.MapGroup($"");

                if (s.Value.Config!.EnableAuth) prefix.RequireAuthorization();

                if (config.Config!.UseRateLimit) prefix.RequireRateLimiting("rate_global");

                foreach (var e in s.Value.Endpoints!)
                {
                    switch (e.Value.Method?.ToUpper())
                    {
                        case "POST":
                            prefix.MapPost(e.Value.Path!,async (HttpRequest request , IHttpService service) =>
                            {
                                var response = await service.HandlePost(s.Value.Config.Origin!, e.Value.Path!, request.Body);
                                return Results.Ok(response);
                            });
                            break;
                        case "GET":
                            var path = ConvertUrl(e.Value.Path!);
                            prefix.MapGet(path,async (HttpRequest request, IHttpService service) =>
                            {
                                var originalUrl = e.Value.Path!;

                                var fullUrl = GetFullUrl(request, originalUrl);

                                var response = await service.HandleGet(s.Value.Config.Origin!, fullUrl);

                                return Results.Ok(response);
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

        public static void MapStaticRoutes(this IEndpointRouteBuilder app, Gateway config)
        {
            app.MapGet("/api/spec", () =>
            {
                return Results.Ok(config);
            });

            app.MapPost("/auth", () =>
            {
                return Results.Ok("Auth endpoint");
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