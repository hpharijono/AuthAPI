using BaseAuthAPI.Models.Dtos;

namespace BaseAuthAPI.Services
{
    public interface IAuthService
    {
        Task<ServiceResult<UserResponse>> RegisterAsync(RegisterRequest request);
        Task<ServiceResult<UserResponse>> LoginAsync(LoginRequest request);
    }
}
