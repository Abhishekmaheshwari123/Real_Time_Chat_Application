using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using ConnectHub.Chat.Infrastructure; // Replace with your actual namespace
using Microsoft.EntityFrameworkCore;
using ConnectHub.Chat.API.Hubs;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. REGISTER THE USER ID PROVIDER (Defined at the bottom of this file)
builder.Services.AddSingleton<IUserIdProvider, EmailBasedUserIdProvider>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<ConnectHub.Chat.Application.Services.ChatService>(client =>
{
    var gatewayBaseUrl = builder.Configuration["ApiGateway:BaseUrl"] ?? "http://localhost:7000";
    client.BaseAddress = new Uri(gatewayBaseUrl);
});

builder.Services.AddControllers();
builder.Services.AddSignalR();

// SWAGGER/OPENAPI CONFIGURATION
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ConnectHub Chat API",
        Version = "v1",
        Description = "Real-time chat service API with SignalR support"
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
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
            new string[] { }
        }
    });
});

// 2. CONFIGURE DATABASE (Ensure this matches your connection string)
builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        x => x.MigrationsAssembly("ConnectHub.Chat.API")
    ));

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

// ==========================
// AUTO-MIGRATIONS
// ==========================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ChatDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database migration.");
    }
}

// 4. CONFIGURE MIDDLEWARE
app.UseCors(policy => policy
    .WithOrigins(
        "http://localhost:5173",
        "http://127.0.0.1:5173",
        "http://localhost:7000",
        "http://127.0.0.1:7000",
        "http://localhost:3000",
        "http://127.0.0.1:3000",
        "https://deft-blancmange-e4b2a1.netlify.app")
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials());

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "ConnectHub Chat API v1");
    options.RoutePrefix = string.Empty; // Sets Swagger UI at root
});

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