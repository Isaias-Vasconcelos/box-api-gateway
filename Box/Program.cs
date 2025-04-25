using Box.Http;
using Box.Middlewares;
using Box.Services;
using Box.Services.Implementations;
using Serilog;
using Log = Serilog.Log;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("log.txt",
        rollingInterval: RollingInterval.Day,
        rollOnFileSizeLimit: true)
    .CreateLogger();

if (!File.Exists("service.json"))
    throw new FileNotFoundException("File 'service.json' not found!");

var json = File.ReadAllText("service.json");
var config = JsonSerializer.Deserialize<Global>(json);

builder.Configuration
    .AddJsonFile("service.json", optional: false, reloadOnChange: true);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddSingleton(config);

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AuthPolicy", policy =>
    policy.RequireAuthenticatedUser());

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddHttpClient<IHttpService, HttpService>();
builder.Services.AddAuthConfig(builder.Configuration);
builder.Services.AddRateLimit(config!);

builder.Services.AddSwaggerGen(options =>
{
    options.MapType<Dictionary<string, string>>(() => new OpenApiSchema
    {
        Type = "object",
        AdditionalPropertiesAllowed = true
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<Error>();

app.MapServiceRoutes();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();

app.Run();
