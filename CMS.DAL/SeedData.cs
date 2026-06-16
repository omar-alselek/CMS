using CMS.Models.Entities;
using CMS.Models.Enums;

namespace CMS.DAL;

public static class SeedData
{
    public static void Initialize(ClinicDbContext context)
    {
        if (context.UserAccounts.Any()) return;

        var manager = new ClinicManager
        {
            FullName = "Omar Admin",
            Phone = "0991234567",
            Email = "omar@clinic.com",
            Department = "Management"
        };

        context.ClinicManagers.Add(manager);
        context.SaveChanges();

        var account = new UserAccount
        {
            PersonId = manager.Id,
            Username = "omar.admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role = UserRole.ClinicManager,
            IsActive = true
        };

        context.UserAccounts.Add(account);
        context.SaveChanges();
        // 2. RECEPTIONIST NEW USER
        var receptionist = new Receptionist
        {
            FullName = "Sally Reception",
            Phone = "0770003333",
            Email = "sally@clinic.com",
            ShiftStart = new TimeSpan(8, 0, 0),  
            ShiftEnd = new TimeSpan(16, 0, 0)    
        };
        context.Receptionists.Add(receptionist);
        context.SaveChanges();

        var recAccount = new UserAccount
        {
            PersonId = receptionist.Id,
            Username = "sally.reception",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Sally@123"),
            Role = UserRole.Receptionist,
            IsActive = true
        };
        context.UserAccounts.Add(recAccount);
        context.SaveChanges();
    }
}