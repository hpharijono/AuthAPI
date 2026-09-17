using BaseAuthAPI.Models;

namespace BaseAuthAPI.Services
{
    public interface ITokenService
    {
        (string Token, DateTime ExpiresAtUtc) CreateToken(User user);
    }
}
