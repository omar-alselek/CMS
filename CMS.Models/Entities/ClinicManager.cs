namespace CMS.Models.Entities;

public class ClinicManager : Person
{
    public string? Department { get; set; }
    public UserAccount? UserAccount { get; set; }
}