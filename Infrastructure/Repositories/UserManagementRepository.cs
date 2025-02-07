using Application.Interfaces.IRepository;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Repositories
{
    public class UserManagementRepository : IUserManagementRepository
    {
        private readonly UserManager<User> _userManager;

        public UserManagementRepository(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IdentityResult> AddRoleToUserAsync(User user, string role)
        {
            return await _userManager.AddToRoleAsync(user, role);
        }

        public async Task<User?> FindByIdAsync(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userManager.Users.ToListAsync();
        }

        public async Task<IList<string>> GetRoleAsync(User user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        public async Task<IEnumerable<User>> SearchUserAsync(Expression<Func<User, bool>> query)
        {
            return await _userManager.Users.Where(query).ToListAsync();
        }

        public async Task<IdentityResult> UpdateUserAsync(User user)
        {
            return await _userManager.UpdateAsync(user);
        }

        public async Task<User?> FindByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<User?> FindByUserNameAsync(string userName)
        {
            return await _userManager.FindByNameAsync(userName);
        }

        public async Task<User?> FindByPhoneNumberAsync(string phoneNumber)
        {
            return await _userManager.Users.Where(user =>  user.PhoneNumber == phoneNumber).FirstOrDefaultAsync();
        }

        public async Task<IdentityResult> RemoveFromRoleAsync(User user, string role)
        {
            return await _userManager.RemoveFromRoleAsync(user, role);
        }
    }
}
