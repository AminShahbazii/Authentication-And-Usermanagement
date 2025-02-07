using Application.Common;
using Application.DTOs;
using Application.Interfaces.IRepository;
using Application.Interfaces.IServices;
using System.ComponentModel.DataAnnotations;

namespace Application.Services
{
    public class LoginService : ILoginService
    {
        private readonly TokenService _tokenService;
        private readonly IUserAuthenticationRepository _userAuthentication;
        public LoginService(TokenService tokenService, IUserAuthenticationRepository userAuthentication)
        {
            _userAuthentication = userAuthentication;
            _tokenService = tokenService;
        }

        public async Task<LoginResult> Login(LoginDto loginDto)
        {

            var isEmail = new EmailAddressAttribute().IsValid(loginDto.UsernameOrEmail);

            var user = isEmail ? await _userAuthentication.FindByEmailAsync(loginDto.UsernameOrEmail)
                               : await _userAuthentication.FindByUserNameAsync(loginDto.UsernameOrEmail);

            if (user == null)
            {
                return LoginResult.Failure(new[] { "User does not exist" });
            }

            if (user.Status == Domain.Enum.UserStatus.banned)
            {
                return LoginResult.Failure(new[] { "User is banned" });
            }

            if (user.Status == Domain.Enum.UserStatus.suspended)
            {
                return LoginResult.Failure(new[] { "User is suspended" });
            }

            var isPasswordMatch = await _userAuthentication.IsPasswordMatchAsync(user, loginDto.Password);

            if (!isPasswordMatch.Succeeded)
            {
                return LoginResult.Failure(new[] { "Password does not match" });
            }

            var token = await _tokenService.Token(user);

            user.LastLoginDate = DateTime.UtcNow;

            await _userAuthentication.UpdateUserAsync(user);

            return LoginResult.SuccessResult(token);
        }
    }
}
