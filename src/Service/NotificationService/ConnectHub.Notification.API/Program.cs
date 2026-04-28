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
        policy.WithOrigins("http://127.0.0.1:5501")
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
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("NotificationDb"),
        sql => sql.EnableRetryOnFailure()
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