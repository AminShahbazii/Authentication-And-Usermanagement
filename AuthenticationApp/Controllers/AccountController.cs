using Application.DTOs;
using Application.Interfaces.IServices;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AuthenticationApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly IRegisterService _registerService;
        private readonly ILoginService _loginService;
        private readonly ILogger<AccountController> _logger;
        public AccountController(IRegisterService registerService, ILoginService loginService, ITokenService tokenService, ILogger<AccountController> logger)
        {
            _loginService = loginService;
            _registerService = registerService;
            _tokenService = tokenService;
            _logger = logger;
        }

        [EnableRateLimiting("register-policy")]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            _logger.LogInformation("Request to register sent");

            if (registerDto == null)
            {
                _logger.LogWarning("Request content was null");
                return BadRequest(new { Description = "Register data is required" });
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Request model is invalid");
                return BadRequest(new
                {
                    Description = "Register data is not valid",
                    Errors = ModelState.Values.SelectMany(error => error.Errors.Select(x => x.ErrorMessage))
                });
            }
            try
            {
                var result = await _registerService.Register(registerDto);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User registered successfully. Email: {Email}", registerDto.Email);
                    return Ok(new { Description = "Register was successful" });
                }
                else
                {
                    _logger.LogError("User registered was not successfully. Email: {Email}", registerDto.Email);

                    return BadRequest(new
                    {
                        Description = "Register was not successful",
                        Errors = result.Errors.Select(error => error.Description)
                    });
                }
            }
            catch (Exception ex)
            {
                return Problem(
                    title : "An error occurred while registering the user",
                    statusCode: StatusCodes.Status500InternalServerError,
                    detail: ex.Message
                    );
            }
        }

        [EnableRateLimiting("login-policy")]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {

            _logger.LogInformation("Request to login sent");


            if (loginDto == null)
            {
                _logger.LogWarning("Request content was null");

                return BadRequest(new { Description = "Login data is required" });
            }
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Request model is invalid");

                return BadRequest(new
                {
                    Description = "Login data is not valid",
                    Errors = ModelState.Values.SelectMany(error => error.Errors.Select(x => x.ErrorMessage)).ToList()
                });
            }
            try
            {
                var result = await _loginService.Login(loginDto);

                if (!result.Success)
                {
                    _logger.LogError("User is unauthorized. User name or email: {uoe}", loginDto.UsernameOrEmail);

                    return Unauthorized(new 
                    {
                        Description = "Login failed",
                        Errors = result.Errors
                    });
                }
                _logger.LogInformation("User login successfully. User name or Email: {uoe}", loginDto.UsernameOrEmail);


                return Ok(new {AccessToken = result.Token.AccessToken, RefreshToken = result.Token.RefreshToken });
            }
            catch (Exception ex)
            {
                return Problem(
                    title: "An error occurred while logging in",
                    statusCode: StatusCodes.Status500InternalServerError,
                    detail: ex.Message
                    );
            }
        }

        [HttpPost]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {

            _logger.LogInformation("Request for refresh token sent");


            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                _logger.LogWarning("Content was null or empty");
                return BadRequest(new { Description = "Login data is required" });
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Request model is invalid");

                return BadRequest(new
                {
                    Description = "Login data is not valid",
                    Errors = ModelState.Values.SelectMany(error => error.Errors.Select(x => x.ErrorMessage)).ToList()
                });
            }

            try
            {
                var tokenResponse = await _tokenService.RefreshTokenAsync(request.RefreshToken);

                _logger.LogInformation("Refresh token  sent");
                return Ok(new { AccessToken = tokenResponse.AccessToken, RefreshToken = tokenResponse.RefreshToken });
            }
            catch(Exception ex) 
            {
                return Problem(
                    title: "Unathorize error occurred while logging in",
                    statusCode: StatusCodes.Status401Unauthorized,
                    detail: ex.Message
                    );
            }
        }
    }
}