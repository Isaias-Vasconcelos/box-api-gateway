using Box.Http;

namespace Box.Routes
{
    public static class RouteMappings
    {
        public static void MapServiceRoutes(this IEndpointRouteBuilder app, IConfiguration config)
        {
            var spec = app.ServiceProvider.GetRequiredService<Global>();

            app.MapPost("/auth", async (HttpRequest request, IHttpService service) =>
            {
                var response = await service.Post(spec.AuthOrigin!, request);
                var responseString = await response.Content.ReadAsStringAsync();
                var json = JsonSerializer.Deserialize<JsonElement>(responseString);

                var isAuthorized = json.GetProperty("isAuthenticated").GetBoolean();

                if (!isAuthorized) return Results.Unauthorized();
                
                var token = Token.GenerateToken(config["Auth:Secret"]!);
                    
                var auth =  new ResponseAuth
                {
                    StatusCode = (int)response.StatusCode,
                    Token = token,
                    Data = json
                };
                    
                return Results.Ok(auth);

            });

            app.MapGet("/spec", () => spec);
        }
    }
}