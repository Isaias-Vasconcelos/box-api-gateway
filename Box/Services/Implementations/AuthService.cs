using Auth = Box.Entities.Auth;

namespace Box.Services.Implementations;

public class AuthService(IHttpService service, IConfiguration configuration) : IAuthService
{
    public async Task<Auth> AuthenticateServiceAsync(Gateway config, Stream body)
    {
        var authInfo = config.Config?.Auth;
        
        var response = await service.HandlePost(authInfo.Origin,authInfo.Path!, body);
        var responseData = await response.Content.ReadAsStringAsync();

        var json = JsonSerializer.Deserialize<JsonElement>(responseData);
        var isAuthenticated = json.GetProperty("isAuthenticated").GetBoolean();

        if (isAuthenticated)
        {
            var secret = configuration["Auth:Secret"];
            var token = Token.GenerateToken(secret!, config.Config.AccessToken);
            
            return new Auth
            {
                StatusCode = (int)response.StatusCode,
                Data = json,
                Token = token,
            };
        }
        else
        {
            return new Auth
            {
                StatusCode = (int)response.StatusCode,
                Data = json,
                Token = "",
            };
        }
    }
}