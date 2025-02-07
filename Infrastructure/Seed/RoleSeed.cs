using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;


namespace Infrastructure.Seed
{
    public static class RoleSeed
    {
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

            var roles = new[] { "SuperAdmin", "Admin", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var adminUsername = "SuperAdmin";
            var adminEmail = "SuperAdmin@gmail.com";
            var adminPhoneNumber = "09154235456";
            var admingUser = await userManager.FindByEmailAsync(adminEmail);


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
            }
        }
    }
}
