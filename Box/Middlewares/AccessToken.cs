namespace Box.Middlewares
{
    public class AccessToken(RequestDelegate next, Gateway gateway)
    {
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                var accessToken = gateway.Config?.AccessToken;
                if (accessToken == "123456")
                    await next(httpContext);
                else 
                    throw new UnauthorizedAccessException("AccessToken is invalid");
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex);
                Console.WriteLine(ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 500;

            var errorData = JsonSerializer.SerializeToElement(new
            {
                error = exception.Message,
                stackTrace = exception.StackTrace
            });

            var response = Helper.Response("api.gateway", context.Request.Path, context.Request.Method, 500, errorData);
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}