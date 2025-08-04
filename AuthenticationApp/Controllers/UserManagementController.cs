using Application.DTOs;
using Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AuthenticationApp.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    [EnableRateLimiting("user-management-policy")]
    [Route("api/[controller]")]
    [ApiController]
    public class UserManagementController : ControllerBase
    {
        private readonly IUserManagementService _userManagementService;


        public UserManagementController(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        [HttpGet("get-all-users")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userManagementService.GetAllUsers();

                if (users == null || !users.Any())
                {
                    return NotFound(new { Description = "No users found." });
                }

                return Ok(users);
            }
            catch (Exception ex)
            {
                return Problem(
                    title: "An error occurred while registering the user",
                    statusCode: StatusCodes.Status500InternalServerError,
                    detail: ex.Message
                    );
            }
        }
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            try
            {
                var users = await _userManagementService.Search(query);

                if (users == null || !users.Any())
                {
                    return NotFound(new { Description = "No users found." });
                }

                return Ok(users);
            }
            catch(Exception ex)
            {
                return Problem(
                    title: "An error occurred while registering the user",
                    statusCode: StatusCodes.Status500InternalServerError,
                    detail: ex.Message
                    );
            }
        }
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("user-to-admin")]
        public async Task<IActionResult> UserToAdmin([FromQuery] string userId)
        { 
            try
            {
                var result = await _userManagementService.UserToAdmin(userId);

                if (result.Succeeded)
                {
                    return Ok(new { Description = "User is now an admin." });
                }

                return NotFound(new { Result = result });
            }
            catch (Exception ex)
            {
                return Problem(
                    title: "An error occurred while registering the user",
                    statusCode: StatusCodes.Status500InternalServerError,
                    detail: ex.Message
                    );
            }
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("admin-to-user")]
        public async Task<IActionResult> AdmintToUser([FromQuery] string userId)
        {
            try
            {
                var result = await _userManagementService.AdminToUser(userId);

                if (result.Succeeded)
                {
                    return Ok(new { Description = "Admin role removed successfully." });
                }

                return NotFound(new { Result = result });
            }
            catch (Exception ex)
            {
                return Problem(
                    title: "An error occurred while registering the user",
                    statusCode: StatusCodes.Status500InternalServerError,
                    detail: ex.Message
                );
            }
        }


        [HttpPost("change-status-user")]
        public async Task<IActionResult> StatusUser([FromQuery] string userId, [FromQuery] ChangeUserStatusDto changeUserStatusDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Error = ModelState.Values.SelectMany(error => error.Errors.Select(x => x.ErrorMessage))
                });
            }
            try
            {
                var result = await _userManagementService.ChangeStatusUser(userId, changeUserStatusDto);
                if (result)
                {
                    return Ok(new { Description = "User status has been changed." });
                }
                return NotFound(new { Description = "User status has not been changed." });
            }
            catch (Exception ex)
            {
                return Problem(
                    title: "An error occurred while registering the user",
                    statusCode: StatusCodes.Status500InternalServerError,
                    detail: ex.Message
                    );
            }
        }

        [HttpPut("edit-user")]
        public async Task<IActionResult> EditUser([FromQuery]string userId, [FromBody] EditUserDto editUserDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Errors = ModelState.Values.SelectMany(error => error.Errors.Select(x => x.ErrorMessage))
                });
            }
            try
            {
                var result = await _userManagementService.EditUser(userId, editUserDto);

                if (result)
                {
                    return Ok("User successfully edited");
                }

                return BadRequest("Failed to edit user");

            }
            catch (Exception ex)
            {
            return Problem(
                title: "An error occurred while registering the user",
                statusCode: StatusCodes.Status500InternalServerError,
                detail: ex.Message
            );
            }
        }
    }
}

