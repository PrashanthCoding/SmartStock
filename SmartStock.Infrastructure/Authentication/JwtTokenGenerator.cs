using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartStock.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartStock.Infrastructure.Authentication;

public class JwtTokenGenerator
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenGenerator(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public (string Token, DateTime ExpiresAtUtc) GenerateToken(User user)
    {
        if (string.IsNullOrWhiteSpace(_jwtSettings.SecretKey) ||
            Encoding.UTF8.GetByteCount(_jwtSettings.SecretKey) < 32)
        {
            throw new InvalidOperationException(
                "JWT SecretKey must contain at least 32 bytes.");
        }

        if (string.IsNullOrWhiteSpace(_jwtSettings.Issuer) ||
            string.IsNullOrWhiteSpace(_jwtSettings.Audience) ||
            _jwtSettings.ExpiryInMinutes <= 0)
        {
            throw new InvalidOperationException(
                "JWT configuration is invalid.");
        }

        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler()
            .WriteToken(token);

        return (tokenString, expiresAt);
    }
}