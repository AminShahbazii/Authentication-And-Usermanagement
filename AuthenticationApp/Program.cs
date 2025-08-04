using Application.Common;
using Application.Interfaces.IRepository;
using Application.Interfaces.IServices;
using Application.Services;
using AuthenticationApp.Middlewares;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.DbContext;
using Infrastructure.Logging;
using Infrastructure.Repositories;
using Infrastructure.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

builder.Services.AddOpenApi();

////////////// Log Configuration //////////////
builder.Host.UseSerilog((context, ConfigurationBinder) =>
{
    ConfigurationBinder.ReadFrom.Configuration(context.Configuration);
});


////////////// Database Configuration //////////////
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


////////////// Dependency Injection Configuration //////////////
builder.Services.AddScoped<IUserAuthenticationRepository, UserAuthenticationRepository>();
builder.Services.AddScoped<IUserManagementRepository, UserManagementRepository>();
builder.Services.AddScoped<IRegisterService, RegisterService>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped(typeof(IAppLogger<>) , typeof(LoggerAdapter<>));
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

////////////// Identity Configuration //////////////
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
}).AddEntityFrameworkStores<ApplicationDbContext>();


////////////// JWT Authentication Configuration //////////////
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidAudience = builder.Configuration["JWT:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SignInKey"])),
        ClockSkew = TimeSpan.Zero

        
    };
});



////////////// Rate Limit Configuration //////////////
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("register-policy", context =>
    {
        var userIp = context.Connection.RemoteIpAddress?.ToString()?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(userIp, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 2,
            Window = TimeSpan.FromSeconds(10),
            QueueLimit = 1,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });

    });
    options.AddPolicy("login-policy", context =>
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value

        ?? context.Connection.RemoteIpAddress?.ToString() 
        ?? "unknown";

        return RateLimitPartition.GetSlidingWindowLimiter(userId, _ => new SlidingWindowRateLimiterOptions
        {
            PermitLimit = 3,
            Window = TimeSpan.FromSeconds(10),
            SegmentsPerWindow = 1,
        });

    });
    options.AddPolicy("user-management-policy", context =>
    {

        var role = context.User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? context.Connection.RemoteIpAddress?.ToString()
            ?? "unknown";

        if (!string.IsNullOrEmpty(role) && role.Contains("Admin"))
        {
            return RateLimitPartition.GetSlidingWindowLimiter(userId, _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromSeconds(5),
                SegmentsPerWindow = 4,
                QueueLimit = 3,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });
        }

        return RateLimitPartition.GetFixedWindowLimiter(userId, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromSeconds(30),
            QueueLimit = 2,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });

    });
});


var app = builder.Build();

//////////////Seed Roles and SuperAdmin//////////////
using (var scop = app.Services.CreateScope())
{
    var service = scop.ServiceProvider;

    try
    {
        await RoleSeed.SeedRolesAsync(service);
    }
    catch (Exception ex)
    {
        var logger = service.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occured while seeding the roles.");
    }
}

if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

app.MapScalarApiReference();

app.UseHttpsRedirection();

app.UseMiddleware<GlobalException>();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseRateLimiter();

app.MapControllers();


app.Run();
