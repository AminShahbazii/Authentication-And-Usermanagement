using Application.Interfaces.IRepository;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace Infrastructure.Seed;

public static class RoleSeed
{
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {

        var logger = serviceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("RoleSeed");


        logger.LogInformation("(RoleSeed) Trying to create super admin");

        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();


        var roles = new[] { "SuperAdmin", "Admin", "User" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Role {Role} created.", role);
            }
        }

        var userId = Guid.NewGuid().ToString();
        var adminUsername = "SuperAdmins";
        var adminEmail = "SuperAdmin2@gmail.com";
        var adminPhoneNumber = "09154235446";



        var existUser = await userManager.Users.Where(u => u.Email == adminEmail
        || u.UserName == adminUsername
        || u.Id == userId).AnyAsync();

        if (!existUser)
        {
            logger.LogInformation("(RoleSeed) Trying to create super admin.");

            var admin = new User
            {
                UserName = adminUsername,
                Email = adminEmail,
                PhoneNumber = adminPhoneNumber,
                RegisterDate = DateTime.Now,
            };


            var result = await userManager.CreateAsync(admin, "Password123?");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
                await userManager.AddToRoleAsync(admin, "SuperAdmin");
                logger.LogInformation("(RoleSeed) SuperAdmin created and roles assigned.");
            }
            else
            {
                logger.LogError("(RoleSeed) Failed to create SuperAdmin: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            logger.LogInformation("(RoleSeed) Super admin is not null");
        }

    }
}
    

