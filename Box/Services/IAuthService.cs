namespace Box.Services;

public interface IAuthService
{
    Task<ResponseAuth> Authenticate(HttpRequest request);
}