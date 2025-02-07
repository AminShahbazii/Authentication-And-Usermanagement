using Application.DTOs;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Application.Interfaces.IRepository;

namespace Infrastructure.Repositories
{
    public class UserAuthenticationRepository : IUserAuthenticationRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        public UserAuthenticationRepository(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public async Task<IdentityResult> AddRoleToUserAsync(User user, string role)
        {
            return await _userManager.AddToRoleAsync(user, role);
        }

        public async Task<IdentityResult> CreateUserAsync(User user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }

        public async Task<User?> FindByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<User?> FindByUserNameAsync(string userName)
        {
            return await _userManager.FindByNameAsync(userName);
        }

        public async Task<IList<string>> GetRolesAsync(User user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        public async Task<CheckUserForRegisterDto?> GetUserByRegisterInfoAsync(Expression<Func<User, bool>> query)
        {
            return await _userManager.Users.Where(query)
                .Select(user => new CheckUserForRegisterDto
                {
                    Email = user.Email,
                    UserName = user.UserName,
                    PhoneNumber = user.PhoneNumber
                })
                .FirstOrDefaultAsync();
        }

        public async Task<SignInResult> IsPasswordMatchAsync(User user, string Password)
        {
            return await _signInManager.CheckPasswordSignInAsync(user, Password, false);
        }


        public async Task UpdateUserAsync(User user)
        {
            await _userManager.UpdateAsync(user);
        }

    }
}
