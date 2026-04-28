using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using ConnectHub.Chat.Infrastructure; // Replace with your actual namespace
using Microsoft.EntityFrameworkCore;
using ConnectHub.Chat.API.Hubs; // Replace with your actual namespace

var builder = WebApplication.CreateBuilder(args);

// 1. REGISTER THE USER ID PROVIDER (Defined at the bottom of this file)
builder.Services.AddSingleton<IUserIdProvider, EmailBasedUserIdProvider>();

builder.Services.AddControllers();
builder.Services.AddSignalR();

// 2. CONFIGURE DATABASE (Ensure this matches your connection string)
builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. CONFIGURE AUTHENTICATION & SIGNALR JWT HANDLING
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };

        // This part is crucial for SignalR to read the token from the URL query string
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

var app = builder.Build();

// 4. CONFIGURE MIDDLEWARE
app.UseCors(policy => policy
    .AllowAnyHeader()
    .AllowAnyMethod()
    .SetIsOriginAllowed(_ => true) // Allows local development requests
    .AllowCredentials());

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chatHub");

app.Run();

/* ==========================================================================
   SUPPORTING CLASSES (Merged into Program.cs)
   ========================================================================== */

public class EmailBasedUserIdProvider : IUserIdProvider
{
    public string GetUserId(HubConnectionContext connection)
    {
        // This ensures Clients.User("email@test.com") works correctly
        return connection.User?.FindFirst(ClaimTypes.Email)?.Value;
    }
}