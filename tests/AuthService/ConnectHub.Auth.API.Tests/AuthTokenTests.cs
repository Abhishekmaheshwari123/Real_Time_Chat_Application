using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ConnectHub.Auth.API.Controllers;
using ConnectHub.Auth.Application.DTOs;
using ConnectHub.Auth.Domain;
using ConnectHub.Auth.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConnectHub.Auth.API.Tests;

[TestClass]
public class AuthTokenTests
{
    [TestMethod]
    public async Task Login_ReturnsToken_WithCorrectClaims()
    {
        using var dbContext = CreateDbContext();
        var user = new User
        {
            UserName = "tester",
            Email = "tester@connecthub.dev",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password")
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var settings = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "THIS_IS_SUPER_SECRET_KEY_123456789_ABCDEF_1234567890",
            ["Jwt:Issuer"] = "ConnectHub",
            ["Jwt:Audience"] = "ConnectHubUsers"
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var controller = new AuthController(dbContext, config);

        var result = await controller.Login(new LoginRequest
        {
            Email = "tester@connecthub.dev",
            Password = "password"
        });

        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        var tokenString = ReadProperty<string>(okResult.Value!, "token");

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(tokenString);

        Assert.AreEqual("ConnectHub", token.Issuer);
        Assert.AreEqual("ConnectHubUsers", token.Audiences.First());
        Assert.AreEqual("tester", token.Claims.First(c => c.Type == ClaimTypes.Name).Value);
        Assert.AreEqual("tester@connecthub.dev", token.Claims.First(c => c.Type == ClaimTypes.Email).Value);
        Assert.AreEqual(user.Id.ToString(), token.Claims.First(c => c.Type == "UserId").Value);
    }

    private static AuthDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase($"auth-token-tests-{Guid.NewGuid()}")
            .Options;
        return new AuthDbContext(options);
    }

    private static T ReadProperty<T>(object value, string propertyName)
    {
        return (T)value.GetType().GetProperty(propertyName)!.GetValue(value)!;
    }
}
