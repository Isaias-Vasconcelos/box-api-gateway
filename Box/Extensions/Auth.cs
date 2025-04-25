namespace Box.Extensions;

public static class Auth
{
    public static void AddAuthConfig(this IServiceCollection services, IConfiguration config)
    {
        var secret = Environment.GetEnvironmentVariable("SECRET") ?? config["Auth:Secret"];
        
        if (string.IsNullOrEmpty(secret))
            throw new Exception("Secret is required for JWT configuration.");

        var key = Encoding.ASCII.GetBytes(secret);
        services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(x =>
        {
            x.RequireHttpsMetadata = false;
            x.SaveToken = true;
            x.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false
            };
        });
    }
}