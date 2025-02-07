using Application.DTOs;
using Domain.Entities;


namespace Application.Mapper
{
    public static class UserMapper
    {
        public static User ToUser(this RegisterDto registerDto)
        {
            return new User
            {
                UserName = registerDto.Username,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Email = registerDto.Email,
                PhoneNumber = registerDto.Phone,
                RegisterDate = DateTime.Now
            };
        }

        public static UserDto ToUserDto(this User user, IList<string> Role)
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = Role,
                PhoneNumber = user.PhoneNumber,
                RegisterDate = user.RegisterDate,
                LastLoginDate = user.LastLoginDate,
                Status = user.Status
            };
        }
    }
}
