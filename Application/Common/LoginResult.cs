
using Application.DTOs;

namespace Application.Common
{
    public class LoginResult
    {
        public TokenResponseDto Token { get; set; } // Access token and refresh token
        public bool Success { get; set; }
        public IEnumerable<string> Errors { get; set; } 


        public static LoginResult SuccessResult(TokenResponseDto tokenResponseDto)
        {
            return new LoginResult { Token = tokenResponseDto, Success = true };
        }

        public static LoginResult Failure(IEnumerable<string> errors)
        {
            return new LoginResult { Errors = errors, Success = false };
        }
    }
}
