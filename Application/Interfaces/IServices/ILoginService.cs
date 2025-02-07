using Application.Common;
using Application.DTOs;
using System;

namespace Application.Interfaces.IServices
{
    public interface ILoginService
    {
        Task<LoginResult> Login(LoginDto loginDto);
    }
}
