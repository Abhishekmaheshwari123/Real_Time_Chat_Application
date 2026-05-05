using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConnectHub.Auth.Application.DTOs;
using ConnectHub.Auth.Infrastructure;
using ConnectHub.Auth.Domain;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Google.Apis.Auth;

namespace ConnectHub.Auth.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthDbContext _context;
    private readonly IConfiguration _config;

    public AuthController(AuthDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleAuthRequest request)
    {
        try
        {
            var clientId = _config["Google:ClientId"]?.Trim();
            if (string.IsNullOrEmpty(clientId))
            {
                Console.WriteLine("DEBUG: Google Client ID is MISSING in config!");
                return StatusCode(500, "Google Client ID not configured in backend");
            }

            Console.WriteLine($"DEBUG: Backend is using Google Client ID: {clientId}");

            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new List<string>() { clientId }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(request.Credential, settings);

            var email = payload.Email;
            var name = payload.Name;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                user = new User
                {
                    UserName = name ?? email.Split('@')[0],
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString())
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            var token = CreateToken(user);

            return Ok(new
            {
                token,
                user = new { user.UserName, user.Email }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GOOGLE LOGIN ERROR: {ex.Message}");
            if (ex.InnerException != null) Console.WriteLine($"INNER ERROR: {ex.InnerException.Message}");
            
            return BadRequest(new { message = "Invalid Google token", error = ex.Message });
        }
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;

        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Email == email);

        if (user == null)
            return NotFound();

        return Ok(new
        {
            user.UserName,
            user.Email
        });
    }

    private string CreateToken(User user)
    {
        var jwtKey = _config["Jwt:Key"] ?? "THIS_IS_SUPER_SECRET_KEY_123456789_ABCDEF_1234567890";
        var issuer = _config["Jwt:Issuer"];
        var audience = _config["Jwt:Audience"];

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Email),
            new Claim("UserId", user.Id.ToString())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey)
        );

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            UserName = request.UserName,
            Email = request.Email,
            PasswordHash = passwordHash
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = "User registered successfully" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
            return BadRequest("User not found");

        bool isValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

        if (!isValid)
            return BadRequest("Invalid password");

        var token = CreateToken(user);

        return Ok(new
        {
            token,
            user = new
            {
                user.UserName,
                user.Email
            }
        });
    }
}