using CMS.Models.Entities;

namespace CMS.BLL.Interfaces;

public interface IAuthService
{
    Task<UserAccount?> LoginAsync(string username, string password);
    Task<string> GenerateTokenAsync(UserAccount user);
}