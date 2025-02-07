
namespace Application.Common
{
    public class LoginResult
    {
        public string Token { get; set; }
        public bool Success { get; set; }
        public IEnumerable<string> Errors { get; set; } 


        public static LoginResult SuccessResult(string token)
        {
            return new LoginResult { Token = token, Success = true };
        }

        public static LoginResult Failure(IEnumerable<string> errors)
        {
            return new LoginResult { Errors = errors, Success = false };
        }
    }
}
