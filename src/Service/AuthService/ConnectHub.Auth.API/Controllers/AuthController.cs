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

namespace ConnectHub.Auth.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

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
        // 1. Claims = user identity data inside token
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("UserId", user.Id.ToString())
        };

        // 2. Secret key (same as Program.cs)
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("THIS_IS_SUPER_SECRET_KEY_123456789")
        );

        // 3. Signing credentials (algorithm)
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // 4. Token structure
        var token = new JwtSecurityToken(
            issuer: "ConnectHub",
            audience: "ConnectHubUsers",
            claims: claims,
            expires: DateTime.Now.AddHours(2),
            signingCredentials: creds
        );

        // 5. Convert token to string
        return new JwtSecurityTokenHandler().WriteToken(token);
    }




    private readonly AuthDbContext _context;

    // ✅ Constructor Injection (BEST PRACTICE)
    public AuthController(AuthDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Test()
    {
        return Ok(new { message = "Auth API is running" });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)   // register
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

        // ✅ THIS LINE TELLS US IF DATA IS REALLY SAVED
        var totalUsers = await _context.Users.CountAsync();

        return Ok(new 
        { 
            message = "User registered successfully",
            totalUsers = totalUsers
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        // 1. Find user
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
            return BadRequest(new { message = "User not found" });

        // 2. Verify password
        bool isValid = BCrypt.Net.BCrypt.Verify(
            request.Password,
            user.PasswordHash
        );

        if (!isValid)
            return BadRequest(new { message = "Invalid password" });

        // 3. Generate JWT token
        var token = CreateToken(user);

        // 4. Return response
        return Ok(new
        {
            message = "Login successful",
            token = token,
            user = new
            {
                user.UserName,
                user.Email
            }
        });
    }
}