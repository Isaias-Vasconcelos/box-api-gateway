using Auth = Box.Entities.Auth;

namespace Box.Services;

public interface IAuthService
{
    Task<Auth> AuthenticateServiceAsync(Gateway config, Stream body);
}