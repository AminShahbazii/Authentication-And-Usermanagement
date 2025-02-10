using Application.Interfaces.IRepository;
using Application.Interfaces.IServices;
using Application.Services;
using Domain.Entities;
using Infrastructure.DbContext;
using Infrastructure.Repositories;
using Infrastructure.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

builder.Services.AddOpenApi();

//////////////Database connection string//////////////
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//////////////DI configuration//////////////
builder.Services.AddScoped<IUserAuthenticationRepository, UserAuthenticationRepository>();
builder.Services.AddScoped<IUserManagementRepository, UserManagementRepository>();
builder.Services.AddScoped<IRegisterService, RegisterService>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();

//////////////Identity configuration//////////////
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
}).AddEntityFrameworkStores<ApplicationDbContext>();

//////////////Authentication configuration//////////////
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SignInKey"]))
    };
});

var _logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();

//////////////Rate limit configuration//////////////
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("register-policy", context =>
    {
        _logger.LogInformation("Register policy is called");
        var userIp = context.Connection.RemoteIpAddress?.ToString()?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(userIp, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromSeconds(1),
            QueueLimit = 4,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });
        _logger.LogInformation("Register policy is finished");
    });
    options.AddPolicy("login-policy", context =>
    {
        _logger.LogInformation("Login policy is called");
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? context.Connection.RemoteIpAddress?.ToString() 
        ?? "unknown";

        return RateLimitPartition.GetSlidingWindowLimiter(userId, _ => new SlidingWindowRateLimiterOptions
        {
            PermitLimit = 8,
            Window = TimeSpan.FromSeconds(16),
            SegmentsPerWindow = 4,
            QueueLimit = 5,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });
        _logger.LogInformation("Login policy is finished");
    });
    options.AddPolicy("user-management-policy", context =>
    {
        _logger.LogInformation("User management policy is called");
        var role = context.User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? context.Connection.RemoteIpAddress?.ToString()
            ?? "unknown";

        if (!string.IsNullOrEmpty(role) && role.Contains("Admin"))
        {
            return RateLimitPartition.GetSlidingWindowLimiter(userId, _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 1,
                Window = TimeSpan.FromSeconds(10),
                SegmentsPerWindow = 4,
                QueueLimit = 5,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });
        }
        _logger.LogInformation("User management policy is finished");
        _logger.LogInformation("User management for users policy is started");
        return RateLimitPartition.GetFixedWindowLimiter(userId, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromSeconds(20),
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
        app.MapScalarApiReference();
    }

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.UseRateLimiter();

app.MapControllers();

app.Run();
