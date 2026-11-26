
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartStock.Application.DTOs.Auth;
using SmartStock.Application.Interfaces.Services;
using SmartStock.Domain.Entities;
using SmartStock.Infrastructure.Authentication;
using SmartStock.Infrastructure.Persistence.Context;

namespace SmartStock.Infrastructure.Identity.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly JwtTokenGenerator _jwtTokenGenerator;

    public AuthService(ApplicationDbContext dbContext, IPasswordHasher<User> passwordHasher, JwtTokenGenerator jwtTokenGenerator)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    // ======================================
    // REGISTER
    // ======================================
    public async Task<string> RegisterAsync(RegisterRequestDto request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var existingUser = await _dbContext.Users
            .AnyAsync(x => x.Email == email);

        if (existingUser)
        {
            throw new InvalidOperationException("Email already registered.");
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = email,
            IsActive = true
        };

        // Hash password before saving
        user.PasswordHash = _passwordHasher.HashPassword(
            user,
            request.Password);

        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        return "User registered successfully.";
    }

    // ======================================
    // LOGIN
    // ======================================
    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Email == email);

        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        // Verify hashed password
        var passwordResult = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password);

        if (passwordResult == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        // Rehash password if the hashing configuration has changed
        if (passwordResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(
                user,
                request.Password);

            await _dbContext.SaveChangesAsync();
        }

        // Generate JWT Token
        var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(user);

        return new AuthResponseDto
        {
            Token = token,
            Email = user.Email,
            FullName = user.FullName,
            ExpiresAtUtc = expiresAt
        };
    }
}
