using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CMS.DAL;
using CMS.BLL.Interfaces;
using CMS.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CMS.BLL.Services;

public class AuthService : IAuthService
{
    private readonly ClinicDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(ClinicDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<UserAccount?> LoginAsync(string username, string password)
    {
        var user = await _context.UserAccounts
            .Include(u => u.Person)
            .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

        if (user == null) return null;

        bool isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        return isValid ? user : null;
    }

    public Task<string> GenerateTokenAsync(UserAccount user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"]!;
        var issuer = jwtSettings["Issuer"]!;
        var audience = jwtSettings["Audience"]!;
        var expireMinutes = int.Parse(jwtSettings["ExpireMinutes"]!);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("PersonId", user.PersonId.ToString()),
            new Claim("FullName", user.Person.FullName)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expireMinutes),
            signingCredentials: creds
        );

        return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
    }
}
// يقوم بالبحث عن المستخدم في قاعدة البيانات، التحقق من كلمة السر AuthService
// يحتوي بيانات المستخدم للسماح له بالدخول للواجهات المحمية. JWT Token ثم انشاء 