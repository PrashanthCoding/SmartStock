using SmartStock.Application.DTOs.Auth;

namespace SmartStock.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(RegisterRequestDto request);

        Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    }
}
