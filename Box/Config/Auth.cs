using System.Text;
using Box.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Box.Config;

public static class Auth
{
    public static void AddAuthConfig(this IServiceCollection services, Gateway config)
    {
        if (string.IsNullOrEmpty(config.AuthInfo?.Secret))
            throw new Exception("AuthInfo.Secret is required for JWT configuration.");

        var key = Encoding.ASCII.GetBytes(config.AuthInfo?.Secret!);
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