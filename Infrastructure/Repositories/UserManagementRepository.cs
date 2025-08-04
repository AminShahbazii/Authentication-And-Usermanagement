using Application.Interfaces.IRepository;
using Domain.Entities;
using Infrastructure.DbContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Repositories
{
    public class UserManagementRepository : IUserManagementRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        public UserManagementRepository(UserManager<User> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IdentityResult> AddRoleToUserAsync(User user, string role)
        {
            return await _userManager.AddToRoleAsync(user, role);
        }

        public async Task<User?> FindByIdAsync(string id)
        {
            return await _context.Users
                .Include(r => r.RefreshToken)
                .FirstOrDefaultAsync(u => u.Id == id);
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
            return await _context.Users
                .Include(r => r.RefreshToken)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> FindByUserNameAsync(string userName)
        {
            return await _context.Users
                .Include(r => r.RefreshToken)
                .FirstOrDefaultAsync(u => u.UserName == userName);
        }

        public async Task<User?> FindByPhoneNumberAsync(string phoneNumber)
        {
            return await _context.Users
                .Include(r => r.RefreshToken)
                .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        }

        public async Task<IdentityResult> RemoveFromRoleAsync(User user, string role)
        {
            return await _userManager.RemoveFromRoleAsync(user, role);
        }
    }
}
