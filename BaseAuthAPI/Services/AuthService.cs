using BaseAuthAPI.Models;
using BaseAuthAPI.Models.Dtos;
using BaseAuthAPI.Repositories;
using Microsoft.AspNetCore.Identity;

namespace BaseAuthAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public AuthService(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        public async Task<ServiceResult<UserResponse>> RegisterAsync(RegisterRequest request)
        {
            var emailExists = await _authRepository.EmailExistsAsync(request.Email);
            if (emailExists)
            {
                return ServiceResult<UserResponse>.Fail("A user with this email already exists.");
            }

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            await _authRepository.AddAsync(user);

            return ServiceResult<UserResponse>.Ok(new UserResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
            });
        }

        public async Task<ServiceResult<UserResponse>> LoginAsync(LoginRequest request)
        {
            var user = await _authRepository.GetByEmailAsync(request.Email);
            if (user is null)
            {
                return ServiceResult<UserResponse>.Fail("Invalid email or password.");
            }

            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (verificationResult == PasswordVerificationResult.Failed)
            {
                return ServiceResult<UserResponse>.Fail("Invalid email or password.");
            }

            return ServiceResult<UserResponse>.Ok(new UserResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
            });
        }
    }
}
