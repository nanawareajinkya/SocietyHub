using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SocietyHub.Application.Common.Features.Users.DTOs;
using SocietyHub.Application.Common.Features.Users.Interfaces;
using SocietyHub.Application.Common.Helpers;
using SocietyHub.Domain.Entities;

namespace SocietyHub.Application.Common.Features.Users.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;
        private readonly IConfiguration _config;

        public UserService(IUserRepository repo, IConfiguration config)
        {
            _repo = repo;
            _config = config;
        }

        // ✅ Register a new user
        public async Task<long> RegisterAsync(UserRegisterDto dto)
        {
            // Hash password
            var (hash, salt) = PasswordHelper.HashPassword(dto.Password);

            // Call repository stored proc for Register
            var userId = await _repo.RegisterAsync(
                dto.Username,
                hash,
                salt,
                dto.Email,
                dto.Phone,
                dto.RoleCode
            );

            return userId;
        }

        // ✅ Login and return JWT token + user info
        public async Task<UserDto?> LoginAsync(UserLoginRequestDto dto, string clientIp, string userAgent)
        {
            // Step 1: Fetch hash/salt from DB using stored procedure
            var loginResult = await _repo.LoginGetHashAsync(dto.Username, clientIp, userAgent);

            if (loginResult.Status != 0)
                return null;

            // Step 2: Verify password using helper
            if (loginResult.PasswordHash is null || loginResult.PasswordSalt is null)
                return null;

            var isValid = PasswordHelper.VerifyPassword(dto.Password, loginResult.PasswordHash, loginResult.PasswordSalt);
            if (!isValid)
                return null;

            // Step 3: Fetch user profile
            var user = await _repo.GetByIdAsync(loginResult.UserId!.Value);
            if (user == null) return null;

            // Step 4: Generate JWT Token
            var token = GenerateJwtToken(user);

            // Step 5: Return user DTO
            return new UserDto(
                user.UserId,
                user.Username,
                user.Email,
                user.Phone,
                user.IsActive
            )
            {
                Token = token
            };
        }

        // ✅ Fetch user details by ID
        public async Task<UserDto?> GetByIdAsync(long id)
        {
            var user = await _repo.GetByIdAsync(id);
            if (user == null) return null;

            return new UserDto(
                user.UserId,
                user.Username,
                user.Email,
                user.Phone,
                user.IsActive
            );
        }

        // ✅ Update user profile (email, phone)
        public async Task UpdateAsync(long id, string? email, string? phone)
        {
            await _repo.UpdateAsync(id, email, phone);
        }

        // ✅ Change password securely
        public async Task ChangePasswordAsync(long id, string currentPassword, string newPassword)
        {
            var user = await _repo.GetByIdAsync(id);
            if (user == null)
                throw new Exception("User not found");

            // Fetch password hash/salt for verification
            var loginResult = await _repo.LoginGetHashAsync(user.Username, "", "");
            if (loginResult.Status != 0)
                throw new Exception("Could not verify user");

            var isMatch = PasswordHelper.VerifyPassword(currentPassword, loginResult.PasswordHash!, loginResult.PasswordSalt!);
            if (!isMatch)
                throw new Exception("Incorrect current password");

            var (newHash, newSalt) = PasswordHelper.HashPassword(newPassword);
            await _repo.ChangePasswordAsync(id, newHash, newSalt);
        }

        // ✅ Helper: Generate JWT Token
        private string GenerateJwtToken(User user)
        {
            var jwtKey = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
                throw new Exception("JWT Key not configured.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim("uid", user.UserId.ToString()),
                new Claim(ClaimTypes.Role, "User"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}