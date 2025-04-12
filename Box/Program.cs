using Box.Middlewares;

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

builder.Services.AddSingleton(config);

builder.Services.AddHttpClient<IHttpService, HttpService>();
builder.Services.AddScoped<IGatewayService, GatewayService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddAuthConfig(builder.Configuration);

if(config.Config!.UseRateLimit)
    builder.Services.AddRateLimit(config);

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = config.AppName, Version = config.Version });

    options.MapType<Dictionary<string, string>>(() => new OpenApiSchema
    {
        Type = "object",
        AdditionalPropertiesAllowed = true
    });
});


var app = builder.Build();

if (config.Config!.UseRateLimit)
    app.UseRateLimiter();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<Error>();
app.UseMiddleware<AccessToken>();

app.MapServiceRoutes();
app.MapStaticRoutes();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
