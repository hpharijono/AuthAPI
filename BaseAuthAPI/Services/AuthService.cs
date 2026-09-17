using BaseAuthAPI.Models;
using BaseAuthAPI.Models.Dtos;
using BaseAuthAPI.Repositories;
using Microsoft.AspNetCore.Identity;

namespace BaseAuthAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly ITokenService _tokenService;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public AuthService(IAuthRepository authRepository, ITokenService tokenService)
        {
            _authRepository = authRepository;
            _tokenService = tokenService;
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

        public async Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request)
        {
            var user = await _authRepository.GetByEmailAsync(request.Email);
            if (user is null)
            {
                return ServiceResult<AuthResponse>.Fail("Invalid email or password.");
            }

            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (verificationResult == PasswordVerificationResult.Failed)
            {
                return ServiceResult<AuthResponse>.Fail("Invalid email or password.");
            }

            var (token, expiresAtUtc) = _tokenService.CreateToken(user);

            return ServiceResult<AuthResponse>.Ok(new AuthResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Token = token,
                ExpiresAtUtc = expiresAtUtc,
            });
        }

        public async Task<List<UserResponse>> GetUsersAsync()
        {
            var users = await _authRepository.GetAllAsync();

            return users.Select(u => new UserResponse
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
            }).ToList();
        }
    }
}
