using Application.DTOs;
using Microsoft.AspNetCore.Identity;


namespace Application.Interfaces.IServices
{
    public interface IUserManagementService
    {
        Task<IEnumerable<UserDto>> GetAllUsers();
        Task<IEnumerable<UserDto>> Search(string query);
        Task<IdentityResult> UserToAdmin(string userId);
        Task<IdentityResult> AdminToUser(string userId);
        Task<bool> ChangeStatusUser(string userId, ChangeUserStatusDto changeUserStatusDto);
        Task<bool> EditUser(string userId, EditUserDto editUserDto);
    }
}
