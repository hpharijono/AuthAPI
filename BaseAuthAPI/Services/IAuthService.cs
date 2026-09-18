using BaseAuthAPI.Models.Dtos;

namespace BaseAuthAPI.Services
{
    public interface IAuthService
    {
        Task<ServiceResult<UserResponse>> RegisterAsync(RegisterRequest request);
        Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request);
        Task<ServiceResult<AuthResponse>> LoginWithGoogleAsync(string googleId, string email, string firstName, string lastName);
        Task<List<UserResponse>> GetUsersAsync();
    }
}
