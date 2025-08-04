using Domain.Enum;
using Microsoft.AspNetCore.Identity;


namespace Domain.Entities
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime? RegisterDate { get; set; } 
        public DateTime? LastLoginDate { get; set; }
        public UserStatus Status { get; set; } = UserStatus.active;

        public RefreshToken? RefreshToken { get; set; } 
    }
}
