using Application.DTOs;
using Application.Interfaces.IServices;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IRegisterService _registerService;
        private readonly ILoginService _loginService;
        public AccountController(IRegisterService registerService, ILoginService loginService)
        {
            _loginService = loginService;
            _registerService = registerService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (registerDto == null)
            {
                return BadRequest(new { Description = "Register data is required" });
            }

            if (!ModelState.IsValid)
            {
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
                    return Ok(new { Description = "Register was successful" });
                }
                else
                {
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
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (loginDto == null)
            {
                return BadRequest(new { Description = "Login data is required" });
            }
            if (!ModelState.IsValid)
            {
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
                    return Unauthorized(new 
                    {
                        Description = "Login failed",
                        Errors = result.Errors
                    });
                }

                return Ok(new {Token = result.Token });
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
    }
}