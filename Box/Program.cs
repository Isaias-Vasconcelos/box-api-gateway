using Box.Entities;
using Box.Routes;
using Box.Services;
using Box.Services.Implementations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Threading.RateLimiting;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

var builder = WebApplication.CreateBuilder(args);

var deserializer = new DeserializerBuilder()
    .WithNamingConvention(CamelCaseNamingConvention.Instance) 
    .Build();

if (!File.Exists("service.yaml"))
    throw new FileNotFoundException("File 'service.yaml' not found!");

var yml = File.ReadAllText("service.yaml");
var config = deserializer.Deserialize<Gateway>(yml);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient<IHttpService, HttpService>();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = config.AppName, Version = config.Version });

    options.MapType<Dictionary<string, string>>(() => new OpenApiSchema
    {
        Type = "object",
        AdditionalPropertiesAllowed = true
    });
});

if (string.IsNullOrEmpty(config.AuthInfo?.Secret))
    throw new Exception("AuthInfo.Secret is required for JWT configuration.");

var key = Encoding.ASCII.GetBytes(config.AuthInfo?.Secret!);
builder.Services.AddAuthentication(x =>
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

if (config.Config!.UseRateLimit)
{
    var rateLimitConfig = config.Config!.RateLimit;

    builder.Services.AddRateLimiter(_ =>
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

var app = builder.Build();

if (config.Config.UseRateLimit)
{
    app.UseRateLimiter();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapServiceRoutes(config);
app.MapStaticRoutes(config);

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
