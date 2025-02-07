using Domain.Enum;
using System.ComponentModel.DataAnnotations;


namespace Application.DTOs
{
    public class ChangeUserStatusDto
    {
        [Required]
        [EnumDataType(typeof(UserStatus), ErrorMessage = "Invalid status. Allowed values: Active, Suspended, Banned.")]
        public UserStatus Status { get; set; }
    }
}
