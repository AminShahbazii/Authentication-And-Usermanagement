using Application.Common;
using Application.DTOs;
using Application.Interfaces.IRepository;
using Application.Interfaces.IServices;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services
{
    public class TokenService : ITokenService
    {

        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _key;
        private readonly IUserAuthenticationRepository _userAuthentication;
        private IUserManagementRepository _userManagementRepository;
        private readonly IAppLogger<TokenService> _logger;



        public TokenService(IConfiguration config,
            IUserAuthenticationRepository userAuthentication,
            IAppLogger<TokenService> logger,
            IUserManagementRepository userManagementRepository,
            IRefreshTokenRepository refreshTokenRepository)
        {
            _userAuthentication = userAuthentication;
            _config = config;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:SignInKey"]!));
            _logger = logger;
            _userManagementRepository = userManagementRepository;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<string> GenerateAccessToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Name, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
            };

            var roles = await _userAuthentication.GetRolesAsync(user);

            foreach(var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddSeconds(20),
                SigningCredentials = creds,
                IssuedAt = DateTime.UtcNow,
                Issuer = _config["JWT:Issuer"],
                Audience = _config["JWT:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            _logger.LogInformation("(token) Token created");

            return tokenHandler.WriteToken(token);
        }

        /// ////////////// Generate Refresh Token

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        /// ////////////// return Access and refresh token

        public async Task<TokenResponseDto> GenerateTokensAsync(User user)
        {

            var accessToken = await GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                Expire = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
            };


            var checkUserRefreshToken = await _userManagementRepository.FindByIdAsync(user.Id);


            if ( checkUserRefreshToken!.RefreshToken is not null
                &&(checkUserRefreshToken.RefreshToken.RevokeAt is not null 
                || checkUserRefreshToken.RefreshToken.Expire > DateTime.UtcNow))
            {
                await _refreshTokenRepository.DeleteAsync(checkUserRefreshToken.RefreshToken);
            }

            await _refreshTokenRepository.AddAsync(refreshTokenEntity);


            return new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }


        ///////////////// Check user realty and refresh token for login withour authentication
        public async Task<TokenResponseDto> RefreshTokenAsync(string refreshToken)
        {
            var existingToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

            if (existingToken is null)
            {
                throw new SecurityException("Invalid refresh token");
            }

            if (existingToken.RevokeAt != null || existingToken.Expire < DateTime.UtcNow)
            {
                throw new SecurityException("Refresh token expired or revoked");
            }

            var user = await _userManagementRepository.FindByIdAsync(existingToken.UserId);

            if (user is null )
            {
                throw new SecurityException("User not found");
            }

            await _refreshTokenRepository.RevokeAsync(existingToken);



            return await GenerateTokensAsync(user);
        }
    }
}