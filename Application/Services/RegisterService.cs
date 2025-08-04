using Application.Common;
using Application.DTOs;
using Application.Interfaces.IRepository;
using Application.Interfaces.IServices;
using Application.Mapper;
using Microsoft.AspNetCore.Identity;


namespace Application.Services
{
    public class RegisterService : IRegisterService
    {
        private readonly IUserAuthenticationRepository _userAuthentication;
        private readonly IAppLogger<RegisterService> _logger;

        public RegisterService(IUserAuthenticationRepository userAuthentication, IAppLogger<RegisterService> logger)
        {
            _userAuthentication = userAuthentication;
            _logger = logger;
        }

        public async Task<IdentityResult> Register(RegisterDto registerDto)
        {

            _logger.LogInformation("(register) Check User Exist.");

            var existUser = await _userAuthentication.GetUserByRegisterInfoAsync(user => user.Email == registerDto.Email 
            || user.PhoneNumber == registerDto.Phone 
            || user.UserName == registerDto.Username);


            if (existUser != null)
            {
                _logger.LogWarning("(register) User already exist: {Email}", existUser.Email!);

                string whatExist = existUser.Email == registerDto.Email ? "Email" 
                    : existUser.PhoneNumber == registerDto.Phone ? "Phone" 
                    : "Username";

                return IdentityResult.Failed(new IdentityError { Description = $"{whatExist} already exists" });
            }

            var user = registerDto.ToUser();

            _logger.LogInformation("(register) Create user: {Email}", user.Email);

            var result = await _userAuthentication.CreateUserAsync(user, registerDto.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("(register) Add role to user: {Email}.", user.Email!);

                var roleResult = await _userAuthentication.AddRoleToUserAsync(user, "User");

                if(!roleResult.Succeeded)
                {
                    var errorTextRole = string.Join(" | ", roleResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
                    _logger.LogError("(register) Error adding role to user: {Email} , and error is : {Error}", user.Email!, errorTextRole);

                    var erros = roleResult.Errors.Select(error => new IdentityError
                    {
                        Description = error.Description,
                        Code = error.Code
                    }).ToArray();

                    return IdentityResult.Failed(erros);
                }

                _logger.LogInformation("(register) User completely registered. Email: {Email}", user.Email);

                return result;
            }
            var errorText = string.Join(" | ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
            _logger.LogError("(register) Error create user: {Email} , and error is: {Error}", user.Email, errorText);

            var errors = result.Errors.Select(error => new IdentityError
            {
                Description = error.Description,
                Code = error.Code
            }).ToArray();

            return IdentityResult.Failed(errors);
        }
    }
}
