using Application.DTOs;
using Microsoft.AspNetCore.Identity;


namespace Application.Interfaces.IServices
{
    public interface IRegisterService
    {
        Task<IdentityResult> Register(RegisterDto registerDto);
    }
}
