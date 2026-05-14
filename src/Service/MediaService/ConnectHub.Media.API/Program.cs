using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ConnectHub.Media.Application.Services;
using ConnectHub.Media.API.Services;
<<<<<<< HEAD
using Microsoft.OpenApi.Models;
=======
>>>>>>> 63943581c9f79be279c2a0841674ffa1d34db81f


var builder = WebApplication.CreateBuilder(args);

// Register dependencies
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();

builder.Services.AddControllers();
builder.Services.AddAuthorization();

<<<<<<< HEAD
// SWAGGER/OPENAPI CONFIGURATION
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ConnectHub Media API",
        Version = "v1",
        Description = "Media storage and retrieval service API"
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

=======
>>>>>>> 63943581c9f79be279c2a0841674ffa1d34db81f
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(
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
});

// Configure Authentication & JWT Handling
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
    });

var app = builder.Build();

<<<<<<< HEAD
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "ConnectHub Media API v1");
    options.RoutePrefix = string.Empty; // Sets Swagger UI at root
});

=======
>>>>>>> 63943581c9f79be279c2a0841674ffa1d34db81f
app.UseRouting();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
