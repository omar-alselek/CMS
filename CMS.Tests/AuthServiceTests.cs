using CMS.BLL.Services;
using CMS.DAL;
using CMS.Models.Entities;
using CMS.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using FluentAssertions;

namespace CMS.Tests;

public class AuthServiceTests
{
    private ClinicDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ClinicDbContext(options);
    }

    private IConfiguration GetFakeConfig()
    {
        var config = new Dictionary<string, string?>
        {
            ["JwtSettings:SecretKey"] = "TestSecretKeyMustBe32CharsMinimum!",
            ["JwtSettings:Issuer"] = "CMS.API",
            ["JwtSettings:Audience"] = "CMS.Client",
            ["JwtSettings:ExpireMinutes"] = "60"
        };
        return new ConfigurationBuilder()
            .AddInMemoryCollection(config)
            .Build();
    }

    private UserAccount CreateTestUser(ClinicDbContext context, bool isActive = true)
    {
        var manager = new ClinicManager
        {
            FullName = "Test Manager",
            Phone = "0991234567",
            Department = "IT"
        };
        context.ClinicManagers.Add(manager);
        context.SaveChanges();

        var user = new UserAccount
        {
            PersonId = manager.Id,
            Username = "test.user",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@123"),
            Role = UserRole.ClinicManager,
            IsActive = isActive
        };
        context.UserAccounts.Add(user);
        context.SaveChanges();
        return user;
    }

    // UC-21/22 — Main Flow
    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsUser()
    {
        var context = GetInMemoryContext();
        CreateTestUser(context);
        var service = new AuthService(context, GetFakeConfig());

        var result = await service.LoginAsync("test.user", "Test@123");

        result.Should().NotBeNull();
        result!.Username.Should().Be("test.user");
    }

    // UC-21 ALT-2 — Wrong Password
    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsNull()
    {
        var context = GetInMemoryContext();
        CreateTestUser(context);
        var service = new AuthService(context, GetFakeConfig());

        var result = await service.LoginAsync("test.user", "WrongPass");

        result.Should().BeNull();
    }

    // UC-21 ALT-1 — Username not found
    [Fact]
    public async Task LoginAsync_UserNotFound_ReturnsNull()
    {
        var context = GetInMemoryContext();
        var service = new AuthService(context, GetFakeConfig());

        var result = await service.LoginAsync("nobody", "Test@123");

        result.Should().BeNull();
    }

    // UC-21 ALT-3 — Inactive account
    [Fact]
    public async Task LoginAsync_InactiveAccount_ReturnsNull()
    {
        var context = GetInMemoryContext();
        CreateTestUser(context, isActive: false);
        var service = new AuthService(context, GetFakeConfig());

        var result = await service.LoginAsync("test.user", "Test@123");

        result.Should().BeNull();
    }

    // Token generation
    [Fact]
    public async Task GenerateTokenAsync_ValidUser_ReturnsNonEmptyToken()
    {
        var context = GetInMemoryContext();
        var user = CreateTestUser(context);
        user.Person = context.ClinicManagers.Find(user.PersonId)!;
        var service = new AuthService(context, GetFakeConfig());

        var token = await service.GenerateTokenAsync(user);

        token.Should().NotBeNullOrEmpty();
        token.Split('.').Should().HaveCount(3); // JWT format
    }
}