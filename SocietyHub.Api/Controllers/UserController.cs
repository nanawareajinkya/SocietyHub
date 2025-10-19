using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocietyHub.Application.Common.Features.Users.DTOs;
using SocietyHub.Application.Common.Features.Users.Interfaces;

namespace SocietyHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // ✅ REGISTER USER
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                    return BadRequest(new { message = "Username and password are required." });

                var newUserId = await _userService.RegisterAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = newUserId }, new { userId = newUserId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ✅ LOGIN (returns JWT token)
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] UserLoginRequestDto dto)
        {
            try
            {
                var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var userAgent = Request.Headers["User-Agent"].ToString();

                var user = await _userService.LoginAsync(dto, clientIp, userAgent);

                if (user == null)
                    return Unauthorized(new { message = "Invalid username or password." });

                return Ok(new
                {
                    user.UserId,
                    user.Username,
                    user.Email,
                    user.Phone,
                    user.IsActive,
                    user.Token
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ✅ GET USER BY ID (secured)
        [HttpGet("{id:long}")]
        [Authorize]
        public async Task<IActionResult> GetById(long id)
        {
            try
            {
                var user = await _userService.GetByIdAsync(id);
                if (user == null)
                    return NotFound(new { message = "User not found." });

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ✅ UPDATE PROFILE
        [HttpPut("{id:long}")]
        [Authorize]
        public async Task<IActionResult> Update(long id, [FromBody] UpdateUserRequest dto)
        {
            try
            {
                await _userService.UpdateAsync(id, dto.Email, dto.Phone);
                return Ok(new { message = "Profile updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ✅ CHANGE PASSWORD
        [HttpPost("{id:long}/change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(long id, [FromBody] ChangePasswordRequest dto)
        {
            try
            {
                await _userService.ChangePasswordAsync(id, dto.CurrentPassword, dto.NewPassword);
                return Ok(new { message = "Password changed successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ✅ Request Models for Update & Password Change
        public class UpdateUserRequest
        {
            public string? Email { get; set; }
            public string? Phone { get; set; }
        }

        public class ChangePasswordRequest
        {
            public string CurrentPassword { get; set; } = string.Empty;
            public string NewPassword { get; set; } = string.Empty;
        }
    }
}
