using Application.Common;
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
        private readonly IAppLogger<UserManagementService> _logger;

        public UserManagementService(IUserManagementRepository userManagement, IAppLogger<UserManagementService> logger)
        {
            _userManagement = userManagement;
            _logger = logger;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsers()
        {

            _logger.LogInformation("(User-Management) Get all users");

            var users = await _userManagement.GetAllUsersAsync();

            if (!users.Any())
            {
                _logger.LogWarning("(User-Management) There is no user");

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

            _logger.LogInformation("(User-Management) Search query: {query}", query);

            var users = await _userManagement.SearchUserAsync(user =>
                   user.Id.ToLower().Contains(query.ToLower())
                || user.Email!.ToLower().Contains(query.ToLower())
                || user.UserName!.ToLower().Contains(query.ToLower())
                || user.PhoneNumber!.ToLower().Contains(query.ToLower()));

            if (!users.Any())
            {
                _logger.LogWarning("(User-Management) There is no user with this query: {query}", query);

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
                _logger.LogError("(User-Management) User id is empty. to convert user to admin");
                return IdentityResult.Failed(new IdentityError { Description = "User id is empty" });
            }

            var user = await _userManagement.FindByIdAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("(User-Management) There is no user with this user id: {userId}. to conver user to admin", userId);
                return IdentityResult.Failed(new IdentityError { Description = "There is no user with this user id" });
            }

            var userRoles = await _userManagement.GetRoleAsync(user);

            if (userRoles.Contains("Admin"))
            {
                _logger.LogError("(User-Management) User already is an admin. User id: {userId} and Email: {Email}", userId, user.Email!);
                return IdentityResult.Failed(new IdentityError { Description = "User is already an admin" });
            }

            _logger.LogInformation("(User-Management) Convert user to Admin: {Email}", user.Email!);

            var result = await _userManagement.AddRoleToUserAsync(user, "Admin");

            return result;
        }

        public async Task<bool> ChangeStatusUser(string userId, ChangeUserStatusDto changeUserStatusDto)
        {
            _logger.LogInformation("(User-Management) Trying to change status user: {userId}", userId);

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("(User-Management) User id is empty or null: {userId}", userId);

                throw new Exception("User id is empty");
            }

            _logger.LogInformation("(User-Management) Try to find user: {userId}", userId);

            var user = await _userManagement.FindByIdAsync(userId);

            if (user == null)
            {
                _logger.LogError("(User-Management) There is no user with this user id: {userId}", userId);

                throw new Exception("There is no user with this user id");
            }

            _logger.LogInformation("(User-Management) User found an try to check its status. User id: {userId} and Email: {Email}", userId, user.Email!);

            UserStatus userStatus = changeUserStatusDto.Status;

            if (userStatus.HasFlag(UserStatus.banned))
            {
                if (user.Status == UserStatus.banned)
                {
                    _logger.LogWarning("(User-Management) User already banned. Email: {Email}", user.Email!);
                    throw new Exception("User is already banned");
                }
                user.Status = Domain.Enum.UserStatus.banned;

                var result = await _userManagement.UpdateUserAsync(user);

                if (result.Succeeded)
                {
                    _logger.LogInformation("(User-Management) User status successfully changed to ban. Email: {Email}", user.Email!);

                    return true;
                }
            }

            if (userStatus.HasFlag(UserStatus.suspended))
            {
                if (user.Status == UserStatus.suspended)
                {
                    _logger.LogWarning("(User-Management) User already suspended. Email: {Email}", user.Email!);

                    throw new Exception("User is already suspended");
                }
                user.Status = Domain.Enum.UserStatus.suspended;

                var result = await _userManagement.UpdateUserAsync(user);

                if (result.Succeeded)
                {
                    _logger.LogInformation("(User-Management) User status successfully changed to suspend. Email: {Email}", user.Email!);


                    return true;
                }
            }

            if (userStatus.HasFlag(UserStatus.active))
            {
                if (user.Status == UserStatus.active)
                {
                    _logger.LogWarning("(User-Management) User already active. Email: {Email}", user.Email!);


                    throw new Exception("User is already active");
                }
                user.Status = Domain.Enum.UserStatus.active;

                var result = await _userManagement.UpdateUserAsync(user);

                if (result.Succeeded)
                {
                    _logger.LogInformation("(User-Management) User status successfully changed to active. Email: {Email}", user.Email!);


                    return true;
                }
            }

            _logger.LogWarning("(User-Management) User status did not change. Email: {Email}", user.Email!);

            return false;

        }

        public async Task<bool> EditUser(string userId, EditUserDto editUserDto)
        {

            _logger.LogInformation("(User-Management) Try to edit user with user id: {userid}", userId);

            var user = await _userManagement.FindByIdAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("(User-Management) There is no user with this user id: {userid}", userId);

                throw new Exception("There is no user with this user id");
            }

            if (!string.IsNullOrEmpty(editUserDto.Username))
            {
                var existUser = await _userManagement.FindByUserNameAsync(editUserDto.Username);
                if (existUser != null)
                {
                    _logger.LogError("(User-Management) A user already exist with this username: {username}", user.UserName!);

                    throw new Exception("A user already exist with this username");
                }

                _logger.LogInformation("(User-Management) User name changed. {exUserName} to {nextUserName}", existUser!.UserName!, editUserDto.Username);

                user.UserName = editUserDto.Username;
            }

            if (!string.IsNullOrEmpty(editUserDto.Email))
            {
                var existUser = await _userManagement.FindByEmailAsync(editUserDto.Email);
                if (existUser != null)
                {
                    _logger.LogError("(User-Management) A user already exist with this Email: {Email}", user.Email!);

                    throw new Exception("A user already exist with this email");
                }

                _logger.LogInformation("(User-Management) Email changed. {exEmail} to {nextEmail}", existUser!.Email!, editUserDto.Email);

                user.Email = editUserDto.Email;
            }


            if (!string.IsNullOrEmpty(editUserDto.PhoneNumber))
            {
                var existUser = await _userManagement.FindByPhoneNumberAsync(editUserDto.PhoneNumber);
                if (existUser != null)
                {

                    _logger.LogError("(User-Management) A user already exist with this phone number: {number}", user.PhoneNumber!);

                    throw new Exception("A user already exist with this phone number");
                }

                _logger.LogInformation("(User-Management) Phone number changed. {exNumber} to {nextNumber}", existUser!.PhoneNumber!, editUserDto.PhoneNumber);

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

            _logger.LogInformation("(User-Management) User edited successfully: {Email}", user.Email!);

            return true;
        }

        public async Task<IdentityResult> AdminToUser(string userId)
        {

            _logger.LogInformation("(User-Management) Trying for admin to user {userId}", userId);

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogInformation("(User-Management) User id is empty");

                return IdentityResult.Failed(new IdentityError { Description = "User id is empty" });
            }

            var user = await _userManagement.FindByIdAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("(User-Management) There is no user with this user id: {userId}", userId);

                return IdentityResult.Failed(new IdentityError { Description = "There is no user with this user id." });
            }

            var userRoles = await _userManagement.GetRoleAsync(user);

            if (!userRoles.Contains("Admin"))
            {
                _logger.LogWarning("(User-Management) User is not an admin. Email: {email}", user.Email!);

                return IdentityResult.Failed(new IdentityError { Description = "User is not an Admin." });
            }

            var removeResult = await _userManagement.RemoveFromRoleAsync(user, "Admin");

            if (!removeResult.Succeeded)
            {
                _logger.LogInformation("(User-Management) Failed to remove Admin role. Email: {Email}", user.Email!);

                return IdentityResult.Failed(new IdentityError { Description = "Failed to remove Admin role." });
            }

            _logger.LogInformation("(User-Management) Converting admin to user was successfully. Email: {Email}", user.Email!);

            return IdentityResult.Success;
        }
    }
}
    
