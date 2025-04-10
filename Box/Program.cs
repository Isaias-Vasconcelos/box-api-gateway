using Box.Entities;
using Box.Routes;
using Box.Config;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

var builder = WebApplication.CreateBuilder(args);

var deserializer = new DeserializerBuilder()
    .WithNamingConvention(CamelCaseNamingConvention.Instance) 
    .Build();

if (!File.Exists("service.yaml"))
    throw new FileNotFoundException("File 'service.yaml' not found!");

var yml = File.ReadAllText("service.yaml");
var configGateway = deserializer.Deserialize<Gateway>(yml);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthConfig(configGateway);
builder.Services.AddRateLimit(configGateway);

var app = builder.Build();

if (configGateway.Config!.UseRateLimit)
{
    app.UseRateLimiter();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapServiceRoutes(configGateway);
app.MapStaticRoutes(configGateway);

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
