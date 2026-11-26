using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStock.Application.DTOs.Auth;
using SmartStock.Application.Interfaces.Services;

namespace SmartStock.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // ======================================
    // POST: api/auth/register
    // ======================================
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequestDto request)
    {
        var result = await _authService.RegisterAsync(request);

        return Ok(new
        {
            Success = true,
            Message = result
        });
    }

    // ======================================
    // POST: api/auth/login
    // ======================================
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);

        return Ok(new
        {
            Success = true,
            Message = "Login successful.",
            Data = result
        });
    }
}
