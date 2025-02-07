using Application.DTOs;
using Application.Interfaces.IRepository;
using Application.Interfaces.IServices;
using Application.Mapper;
using Domain.Enum;
using Microsoft.AspNetCore.Identity;

namespace Application.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly IUserManagementRepository _userManagement;


        public UserManagementService(IUserManagementRepository userManagement)
        {
            _userManagement = userManagement;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsers()
        {
            var users = await _userManagement.GetAllUsersAsync();

            if (!users.Any())
            {
                return new List<UserDto>();
            }

            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await _userManagement.GetRoleAsync(user);
                userDtos.Add(user.ToUserDto(roles));
            }

            return userDtos;

        }

        public async Task<IEnumerable<UserDto>> Search(string query)
        {
            var users = await _userManagement.SearchUserAsync(user =>
                   user.Id.ToLower().Contains(query.ToLower())
                || user.Email.ToLower().Contains(query.ToLower())
                || user.UserName.ToLower().Contains(query.ToLower())
                || user.PhoneNumber.ToLower().Contains(query.ToLower()));

            if (!users.Any())
            {
                return new List<UserDto>();
            }

            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await _userManagement.GetRoleAsync(user);
                userDtos.Add(user.ToUserDto(roles));
            }

            return userDtos;
        }

        public async Task<IdentityResult> UserToAdmin(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return IdentityResult.Failed(new IdentityError { Description = "User id is empty" });
            }

            var user = await _userManagement.FindByIdAsync(userId);

            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "There is no user with this user id" });
            }

            var userRoles = await _userManagement.GetRoleAsync(user);

            if (userRoles.Contains("Admin"))
            {
                return IdentityResult.Failed(new IdentityError { Description = "User is already an admin" });
            }

            var result = await _userManagement.AddRoleToUserAsync(user, "Admin");

            return result;
        }

        public async Task<bool> ChangeStatusUser(string userId, ChangeUserStatusDto changeUserStatusDto)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new Exception("User id is empty");
            }

            var user = await _userManagement.FindByIdAsync(userId);

            if (user == null)
            {
                throw new Exception("There is no user with this user id");
            }

            UserStatus userStatus = changeUserStatusDto.Status;

            if (userStatus.HasFlag(UserStatus.banned))
            {
                if (user.Status == UserStatus.banned)
                {
                    throw new Exception("User is already banned");
                }
                user.Status = Domain.Enum.UserStatus.banned;

                var result = await _userManagement.UpdateUserAsync(user);

                if (result.Succeeded)
                {
                    return true;
                }
            }

            if (userStatus.HasFlag(UserStatus.suspended))
            {
                if (user.Status == UserStatus.suspended)
                {
                    throw new Exception("User is already suspended");
                }
                user.Status = Domain.Enum.UserStatus.suspended;

                var result = await _userManagement.UpdateUserAsync(user);

                if (result.Succeeded)
                {
                    return true;
                }
            }

            if (userStatus.HasFlag(UserStatus.active))
            {
                if (user.Status == UserStatus.active)
                {
                    throw new Exception("User is already active");
                }
                user.Status = Domain.Enum.UserStatus.active;

                var result = await _userManagement.UpdateUserAsync(user);

                if (result.Succeeded)
                {
                    return true;
                }
            }

            return false;

        }

        public async Task<bool> EditUser(string userId, EditUserDto editUserDto)
        {
            var user = await _userManagement.FindByIdAsync(userId);

            if (user == null)
            {
                throw new Exception("There is no user with this user id");
            }

            if (!string.IsNullOrEmpty(editUserDto.Username))
            {
                var existUser = await _userManagement.FindByUserNameAsync(editUserDto.Username);
                if (existUser != null)
                {
                    throw new Exception("A user already exist with this username");
                }
                user.UserName = editUserDto.Username;
            }

            if (!string.IsNullOrEmpty(editUserDto.Email))
            {
                var existUser = await _userManagement.FindByEmailAsync(editUserDto.Email);
                if (existUser != null)
                {
                    throw new Exception("A user already exist with this email");
                }
                user.Email = editUserDto.Email;
            }


            if (!string.IsNullOrEmpty(editUserDto.PhoneNumber))
            {
                var existUser = await _userManagement.FindByPhoneNumberAsync(editUserDto.PhoneNumber);
                if (existUser != null)
                {
                    throw new Exception("A user already exist with this phone number");
                }
                user.PhoneNumber = editUserDto.PhoneNumber;
            }

            if (!string.IsNullOrEmpty(editUserDto.FirstName))
            {
                user.FirstName = editUserDto.FirstName;
            }


            if (!string.IsNullOrEmpty(editUserDto.LastName))
            {
                user.LastName = editUserDto.LastName;
            }

            await _userManagement.UpdateUserAsync(user);

            return true;
        }

        public async Task<IdentityResult> AdminToUser(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return IdentityResult.Failed(new IdentityError { Description = "User id is empty" });
            }

            var user = await _userManagement.FindByIdAsync(userId);

            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "There is no user with this user id." });
            }

            var userRoles = await _userManagement.GetRoleAsync(user);

            if (!userRoles.Contains("Admin"))
            {
                return IdentityResult.Failed(new IdentityError { Description = "User is not an Admin." });
            }

            var removeResult = await _userManagement.RemoveFromRoleAsync(user, "Admin");

            if (!removeResult.Succeeded)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Failed to remove Admin role." });
            }

            return IdentityResult.Success;
        }
    }
}
    
