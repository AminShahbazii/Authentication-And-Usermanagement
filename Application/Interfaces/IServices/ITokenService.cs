

using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces.IServices
{
    public interface ITokenService
    {
        Task<TokenResponseDto> RefreshTokenAsync(string refreshToken);
        Task<TokenResponseDto> GenerateTokensAsync(User user);
        Task<string> GenerateAccessToken(User user);
        string GenerateRefreshToken();
    }
}
