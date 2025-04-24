using Box.Http;

namespace Box.Services.Implementations;

public class AuthService(IHttpService http, IConfiguration config, Global spec):IAuthService
{
    public async Task<ResponseAuth> Authenticate(HttpRequest request)
    {
        var response = await http.Post(spec.AuthOrigin!, request);
        var responseString = await response.Content.ReadAsStringAsync();
        
        var json = JsonSerializer.Deserialize<JsonElement>(responseString);
        
        var isAuthorized = json.GetProperty("isAuthenticated").GetBoolean();
        var token = isAuthorized ? Token.GenerateToken(config["Auth:Secret"]!) : "";
                    
        var auth =  new ResponseAuth
        {
            StatusCode = (int)response.StatusCode,
            Token = token,
            Data = json
        };
        
        return auth;
    }
}