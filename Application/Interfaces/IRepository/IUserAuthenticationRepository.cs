using Application.DTOs;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.Linq.Expressions;


namespace Application.Interfaces.IRepository
{
    public interface IUserAuthenticationRepository
    {
        Task<IdentityResult> CreateUserAsync(User user, string password);
        Task UpdateUserAsync(User user);
        Task<CheckUserForRegisterDto?> GetUserByRegisterInfoAsync(Expression<Func<User, bool>> query);
        Task<IdentityResult> AddRoleToUserAsync(User user, string role);
        Task<SignInResult> IsPasswordMatchAsync(User user, string Password);
        Task<IList<string>> GetRolesAsync(User user);

        Task<User?> FindByEmailAsync(string email);
        Task<User?> FindByUserNameAsync(string userName);
    }
}
