using BaseAuthAPI.Models.Dtos;

namespace BaseAuthAPI.Services
{
    public interface IAuthService
    {
        Task<ServiceResult<UserResponse>> RegisterAsync(RegisterRequest request);
        Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request);
        Task<List<UserResponse>> GetUsersAsync();
    }
}
