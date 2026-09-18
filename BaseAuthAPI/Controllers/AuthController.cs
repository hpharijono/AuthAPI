using System.Security.Claims;
using BaseAuthAPI.Models.Dtos;
using BaseAuthAPI.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;

namespace BaseAuthAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IValidator<RegisterRequest> _registerValidator;
        private readonly IValidator<LoginRequest> _loginValidator;
        private readonly GoogleAuthOptionsStatus _googleAuthOptionsStatus;

        public AuthController(
            IAuthService authService,
            IValidator<RegisterRequest> registerValidator,
            IValidator<LoginRequest> loginValidator,
            GoogleAuthOptionsStatus googleAuthOptionsStatus)
        {
            _authService = authService;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
            _googleAuthOptionsStatus = googleAuthOptionsStatus;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserResponse>> Register(RegisterRequest request)
        {
            var validationResult = await _registerValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
            }

            var result = await _authService.RegisterAsync(request);
            if (!result.Success)
            {
                return BadRequest(new { errors = result.Errors });
            }

            return Ok(result.Data);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        {
            var validationResult = await _loginValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
            }

            var result = await _authService.LoginAsync(request);
            if (!result.Success)
            {
                return BadRequest(new { errors = result.Errors });
            }

            return Ok(result.Data);
        }

        [HttpGet("google/login")]
        public IActionResult GoogleLogin()
        {
            if (!_googleAuthOptionsStatus.IsConfigured)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    message = "Google login is not configured. Set Google:ClientId and Google:ClientSecret via user secrets."
                });
            }

            var redirectUrl = Url.Action(nameof(GoogleComplete), "Auth");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google/complete")]
        public async Task<ActionResult<AuthResponse>> GoogleComplete()
        {
            if (!_googleAuthOptionsStatus.IsConfigured)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    message = "Google login is not configured. Set Google:ClientId and Google:ClientSecret via user secrets."
                });
            }

            var authenticateResult = await HttpContext.AuthenticateAsync("External");
            if (!authenticateResult.Succeeded || authenticateResult.Principal is null)
            {
                return BadRequest(new { message = "Google authentication failed." });
            }

            var principal = authenticateResult.Principal;
            var googleId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = principal.FindFirstValue(ClaimTypes.Email);
            var firstName = principal.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty;
            var lastName = principal.FindFirstValue(ClaimTypes.Surname) ?? string.Empty;

            await HttpContext.SignOutAsync("External");

            if (googleId is null || email is null)
            {
                return BadRequest(new { message = "Google account did not return the required information." });
            }

            var result = await _authService.LoginWithGoogleAsync(googleId, email, firstName, lastName);
            if (!result.Success)
            {
                return BadRequest(new { errors = result.Errors });
            }

            return Ok(result.Data);
        }
    }
}
