using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.Linq.Expressions;

namespace Application.Interfaces.IRepository
{
    public interface IUserManagementRepository
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<IEnumerable<User>> SearchUserAsync(Expression<Func<User, bool>> query);
        Task<User?> FindByIdAsync(string id);

        Task<IList<string>> GetRoleAsync(User user);

        Task<IdentityResult> AddRoleToUserAsync(User user, string role);

        Task<IdentityResult> UpdateUserAsync(User user);

        Task<User?> FindByEmailAsync(string email);
        Task<User?> FindByUserNameAsync(string userName);

        Task<User?> FindByPhoneNumberAsync(string phoneNumber);

        Task<IdentityResult> RemoveFromRoleAsync(User user, string role);


    }
}
