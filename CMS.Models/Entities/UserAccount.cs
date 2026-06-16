using System.Text.Json.Serialization;
using CMS.Models.Enums;

namespace CMS.Models.Entities;

public class UserAccount : BaseEntity
{
    public int PersonId { get; set; }
    public Person Person { get; set; } = null!;
    public string Username { get; set; } = string.Empty;

    [JsonIgnore]
    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
}