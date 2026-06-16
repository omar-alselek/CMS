using CMS.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _authService.LoginAsync(request.Username, request.Password);

        if (user == null)
            return Unauthorized(new { message = "Invalid username or password." });

        var token = await _authService.GenerateTokenAsync(user);

        return Ok(new
        {
            token,
            role = user.Role.ToString(),
            fullName = user.Person.FullName,
            userId = user.Id,
            personId = user.PersonId  // ← أضف هذا
        });
    }
}

public record LoginRequest(string Username, string Password);