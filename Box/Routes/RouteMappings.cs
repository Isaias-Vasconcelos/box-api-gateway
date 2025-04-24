using Box.Http;
using Box.Services;

namespace Box.Routes
{
    public static class RouteMappings
    {
        public static void MapServiceRoutes(this IEndpointRouteBuilder app)
        {
            var spec = app.ServiceProvider.GetRequiredService<Global>();

            app.MapPost("/auth", async (HttpRequest request, IAuthService service) =>
            {
                var auth = await service.Authenticate(request);
                return string.IsNullOrEmpty(auth.Token) ? Results.Unauthorized() : Results.Ok(auth);
            });

            app.MapGet("/spec", () => spec);
        }
    }
}