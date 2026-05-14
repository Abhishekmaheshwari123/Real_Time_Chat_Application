using System.Security.Claims;
using ConnectHub.Auth.API.Controllers;
using ConnectHub.Auth.Application.DTOs;
using ConnectHub.Auth.Domain;
using ConnectHub.Auth.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConnectHub.Auth.API.Tests;

[TestClass]
public class AuthControllerTests
{
    [TestMethod]
    public async Task GoogleLogin_Returns500_WhenGoogleClientIdMissing()
    {
        using var dbContext = CreateDbContext();
        var controller = CreateController(dbContext, new Dictionary<string, string?>());

        var result = await controller.GoogleLogin(new GoogleAuthRequest { Credential = "token" });

        var objectResult = result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        Assert.AreEqual("Google Client ID not configured in backend", objectResult.Value);
    }

    [TestMethod]
    public async Task GetProfile_ReturnsNotFound_WhenUserDoesNotExist()
    {
        using var dbContext = CreateDbContext();
        var controller = CreateController(dbContext, DefaultJwtSettings());
        SetUser(controller, "missing@connecthub.dev");

        var result = await controller.GetProfile();

        Assert.IsInstanceOfType<NotFoundResult>(result);
    }

    [TestMethod]
    public async Task GetProfile_ReturnsOk_WhenUserExists()
    {
        using var dbContext = CreateDbContext();
        dbContext.Users.Add(new User
        {
            UserName = "alpha",
            Email = "alpha@connecthub.dev",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("pw")
        });
        await dbContext.SaveChangesAsync();

        var controller = CreateController(dbContext, DefaultJwtSettings());
        SetUser(controller, "alpha@connecthub.dev");

        var result = await controller.GetProfile();

        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual("alpha", ReadProperty<string>(okResult.Value!, "UserName"));
        Assert.AreEqual("alpha@connecthub.dev", ReadProperty<string>(okResult.Value!, "Email"));
    }

    [TestMethod]
    public async Task Register_CreatesUserAndReturnsOk()
    {
        using var dbContext = CreateDbContext();
        var controller = CreateController(dbContext, DefaultJwtSettings());

        var result = await controller.Register(new RegisterRequest
        {
            UserName = "new-user",
            Email = "new-user@connecthub.dev",
            Password = "secret"
        });

        Assert.IsInstanceOfType<OkObjectResult>(result);

        var createdUser = await dbContext.Users.SingleAsync(x => x.Email == "new-user@connecthub.dev");
        Assert.AreEqual("new-user", createdUser.UserName);
        Assert.AreNotEqual("secret", createdUser.PasswordHash);
        Assert.IsTrue(BCrypt.Net.BCrypt.Verify("secret", createdUser.PasswordHash));
    }

    [TestMethod]
    public async Task Login_ReturnsBadRequest_WhenUserNotFound()
    {
        using var dbContext = CreateDbContext();
        var controller = CreateController(dbContext, DefaultJwtSettings());

        var result = await controller.Login(new LoginRequest
        {
            Email = "none@connecthub.dev",
            Password = "pw"
        });

        var badRequest = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest);
        Assert.AreEqual("User not found", badRequest.Value);
    }

    [TestMethod]
    public async Task Login_ReturnsBadRequest_WhenPasswordInvalid()
    {
        using var dbContext = CreateDbContext();
        dbContext.Users.Add(new User
        {
            UserName = "beta",
            Email = "beta@connecthub.dev",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correct-pw")
        });
        await dbContext.SaveChangesAsync();

        var controller = CreateController(dbContext, DefaultJwtSettings());

        var result = await controller.Login(new LoginRequest
        {
            Email = "beta@connecthub.dev",
            Password = "wrong-pw"
        });

        var badRequest = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest);
        Assert.AreEqual("Invalid password", badRequest.Value);
    }

    [TestMethod]
    public async Task Login_ReturnsTokenAndUser_WhenCredentialsValid()
    {
        using var dbContext = CreateDbContext();
        dbContext.Users.Add(new User
        {
            UserName = "gamma",
            Email = "gamma@connecthub.dev",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("good-pw")
        });
        await dbContext.SaveChangesAsync();

        var controller = CreateController(dbContext, DefaultJwtSettings());

        var result = await controller.Login(new LoginRequest
        {
            Email = "gamma@connecthub.dev",
            Password = "good-pw"
        });

        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);

        var token = ReadProperty<string>(okResult.Value!, "token");
        var user = ReadProperty<object>(okResult.Value!, "user");

        Assert.IsFalse(string.IsNullOrWhiteSpace(token));
        Assert.AreEqual("gamma@connecthub.dev", ReadProperty<string>(user, "Email"));
    }

    private static AuthController CreateController(AuthDbContext dbContext, IDictionary<string, string?> settings)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        return new AuthController(dbContext, config);
    }

    private static AuthDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase($"auth-tests-{Guid.NewGuid()}")
            .Options;

        return new AuthDbContext(options);
    }

    private static Dictionary<string, string?> DefaultJwtSettings()
    {
        return new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "THIS_IS_SUPER_SECRET_KEY_123456789_ABCDEF_1234567890",
            ["Jwt:Issuer"] = "ConnectHub",
            ["Jwt:Audience"] = "ConnectHubUsers"
        };
    }

    private static void SetUser(ControllerBase controller, string email)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.NameIdentifier, email)
        };

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
            }
        };
    }

    private static T ReadProperty<T>(object value, string propertyName)
    {
        return (T)value.GetType().GetProperty(propertyName)!.GetValue(value)!;
    }
}