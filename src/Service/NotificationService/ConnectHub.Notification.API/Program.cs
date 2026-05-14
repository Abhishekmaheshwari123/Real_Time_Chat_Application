using Microsoft.AspNetCore.SignalR;
using ConnectHub.Notification.Application.Interfaces;
using ConnectHub.Notification.Application.Services;
using ConnectHub.Notification.Domain.Entities;
using ConnectHub.Notification.Infrastructure.Persistence;
using ConnectHub.Notification.Infrastructure.Persistence.Repositories;
using ConnectHub.Notification.API.Hubs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ========================
// CORS
// ========================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "http://127.0.0.1:5173",
                "http://localhost:7000",
                "http://127.0.0.1:7000",
                "http://localhost:3000",
                "http://127.0.0.1:3000",
                "http://127.0.0.1:5501",
                "http://localhost:5501",
                "https://deft-blancmange-e4b2a1.netlify.app")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ========================
// JWT CONFIG
// ========================
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Missing configuration value: Jwt:Key");

var key = Encoding.UTF8.GetBytes(jwtKey);

if (key.Length < 32)
    throw new Exception("JWT Key must be at least 32 characters long");

// ========================
// AUTHENTICATION
// ========================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/notificationHub"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),

        // 🔥 IMPORTANT FOR SIGNALR USER MAPPING
        NameClaimType = ClaimTypes.NameIdentifier
    };
});

builder.Services.AddAuthorization();

// ========================
// DB
// ========================
builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("NotificationDb"),
        x => x.MigrationsAssembly("ConnectHub.Notification.API")
    ));

// ========================
// SERVICES
// ========================
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

// ========================
// SIGNALR (IMPORTANT ORDER)
// ========================
builder.Services.AddSignalR();

// 🔥 THIS IS CRITICAL FIX
builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();

// ========================
// SWAGGER
// ========================
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ConnectHub.Notification.API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// ==========================
// AUTO-MIGRATIONS
// ==========================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<NotificationDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database migration.");
    }
}

// ========================
// PIPELINE
// ========================
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ========================
// TEST TOKEN
// ========================
app.MapGet("/generate-token", () =>
{
    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, "user1")
    };

    var creds = new SigningCredentials(
        new SymmetricSecurityKey(key),
        SecurityAlgorithms.HmacSha256
    );

    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.UtcNow.AddHours(1),
        signingCredentials: creds
    );

    return Results.Ok(new
    {
        token = new JwtSecurityTokenHandler().WriteToken(token)
    });
});

// ========================
// NOTIFICATION API
// ========================
app.MapPost("/notifications", async (
    INotificationService service,
    IHubContext<NotificationHub> hub,
    Notification notification) =>
{
    await service.CreateNotification(notification);

    var unreadCount = await service.GetUnreadCount(notification.RecipientId);

    await hub.Clients.User(notification.RecipientId)
        .SendAsync("ReceiveNotification", new
        {
            message = notification.Message,
            count = unreadCount
        });

    return Results.Ok();
})
.RequireAuthorization();

// ========================
// SIGNALR HUB
// ========================
app.MapHub<NotificationHub>("/notificationHub");

app.MapGet("/favicon.ico", () => Results.NoContent());

app.Run();