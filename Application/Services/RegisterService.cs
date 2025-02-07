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

        public RegisterService(IUserAuthenticationRepository userAuthentication)
        {
            _userAuthentication = userAuthentication;
        }

        public async Task<IdentityResult> Register(RegisterDto registerDto)
        {

            var existUser = await _userAuthentication.GetUserByRegisterInfoAsync(user => user.Email == registerDto.Email 
            || user.PhoneNumber == registerDto.Phone 
            || user.UserName == registerDto.Username);


            if (existUser != null)
            {
                string whatExist = existUser.Email == registerDto.Email ? "Email" 
                    : existUser.PhoneNumber == registerDto.Phone ? "Phone" 
                    : "Username";

                return IdentityResult.Failed(new IdentityError { Description = $"{whatExist} already exists" });
            }

            var user = registerDto.ToUser();

            var result = await _userAuthentication.CreateUserAsync(user, registerDto.Password);

            if (result.Succeeded)
            {
                var roleResult = await _userAuthentication.AddRoleToUserAsync(user, "User");

                if(!roleResult.Succeeded)
                {
                    var erros = roleResult.Errors.Select(error => new IdentityError
                    {
                        Description = error.Description,
                        Code = error.Code
                    }).ToArray();
                    return IdentityResult.Failed(erros);
                }

                return result;
            }

            var errors = result.Errors.Select(error => new IdentityError
            {
                Description = error.Description,
                Code = error.Code
            }).ToArray();

            return IdentityResult.Failed(errors);
        }
    }
}
