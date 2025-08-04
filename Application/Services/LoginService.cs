using Application.Common;
using Application.DTOs;
using Application.Interfaces.IRepository;
using Application.Interfaces.IServices;
using System.ComponentModel.DataAnnotations;

namespace Application.Services
{
    public class LoginService : ILoginService
    {
        private readonly ITokenService _tokenService;
        private readonly IUserAuthenticationRepository _userAuthentication;
        private readonly IAppLogger<LoginService> _logger;
        public LoginService(ITokenService tokenService, IUserAuthenticationRepository userAuthentication, IAppLogger<LoginService> logger)
        {
            _userAuthentication = userAuthentication;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<LoginResult> Login(LoginDto loginDto)
        {

            _logger.LogInformation("(login) Start to login. User name or email: {uoe}", loginDto.UsernameOrEmail);

            var isEmail = new EmailAddressAttribute().IsValid(loginDto.UsernameOrEmail);

            var user = isEmail ? await _userAuthentication.FindByEmailAsync(loginDto.UsernameOrEmail)
                               : await _userAuthentication.FindByUserNameAsync(loginDto.UsernameOrEmail);

            if (user == null)
            {
                _logger.LogWarning("(login) there is no user with this email or username : {ueo}", loginDto.UsernameOrEmail);

                return LoginResult.Failure(new[] { "User does not exist" });
            }

            _logger.LogInformation("(login) Check user status: {Email}", user.Email!);

            if (user.Status == Domain.Enum.UserStatus.banned)
            {

                _logger.LogWarning("(login) User is banned: {Email}", user.Email!);

                return LoginResult.Failure(new[] { "User is banned" });
            }

            if (user.Status == Domain.Enum.UserStatus.suspended)
            {
                _logger.LogWarning("(login) User is suspended: {Email}", user.Email!);

                return LoginResult.Failure(new[] { "User is suspended" });
            }

            _logger.LogInformation("(login) Check password match: {Email}", user.Email!);

            var isPasswordMatch = await _userAuthentication.IsPasswordMatchAsync(user, loginDto.Password);

            if (!isPasswordMatch.Succeeded)
            {

                _logger.LogWarning("(login) Password does not match: {Email}", user.Email!);

                return LoginResult.Failure(new[] { "Password does not match" });
            }

            _logger.LogInformation("(login) Start to create token for user : {Email}", user.Email!);

            var token = await _tokenService.GenerateTokensAsync(user);

            user.LastLoginDate = DateTime.UtcNow;

            await _userAuthentication.UpdateUserAsync(user);

            _logger.LogInformation("(login) User successfully login: {Email}", user.Email!);

            return LoginResult.SuccessResult(token);
        }
    }
}
