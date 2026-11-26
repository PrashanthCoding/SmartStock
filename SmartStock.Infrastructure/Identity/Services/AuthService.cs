using Microsoft.AspNetCore.Identity;
using SmartStock.Application.DTOs.Auth;
using SmartStock.Application.Interfaces.Services;
using SmartStock.Infrastructure.Authentication;

namespace SmartStock.Infrastructure.Identity.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtTokenGenerator _jwtTokenGenerator;

    public AuthService(UserManager<ApplicationUser> userManager, JwtTokenGenerator jwtTokenGenerator)
    {
        _userManager = userManager;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    // ======================================
    // REGISTER
    // ======================================
    public async Task<string> RegisterAsync(RegisterRequestDto request)
    {
        var email = request.Email.Trim();

        var existingUser = await _userManager.FindByEmailAsync(email);

        if (existingUser != null)
        {
            throw new InvalidOperationException("Email already registered.");
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = request.FullName.Trim(),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(x => x.Description));

            throw new InvalidOperationException(errors);
        }

        return "User registered successfully.";
    }

    // ======================================
    // LOGIN
    // ======================================
    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var email = request.Email.Trim();

        var user = await _userManager.FindByEmailAsync(email);

        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);

        if (!isPasswordValid)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        // Generate JWT Token
        var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(user);

        return new AuthResponseDto
        {
            Token = token,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName ?? string.Empty,
            ExpiresAtUtc = expiresAt
        };
    }
}