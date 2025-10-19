using SocietyHub.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocietyHub.Application.Common.Features.Users.Interfaces
{
    public interface IUserRepository
    {
        Task<long> RegisterAsync(string username, byte[] passwordHash, byte[] passwordSalt, string? email, string? phone, string? roleCode);
        Task<(int Status, string Message, byte[]? PasswordHash, byte[]? PasswordSalt, long? UserId)> LoginGetHashAsync(string username, string clientIp, string userAgent);
        Task<User?> GetByIdAsync(long userId);
        Task<User?> GetByUsernameAsync(string username);
        Task UpdateAsync(long userId, string? email, string? phone);
        Task ChangePasswordAsync(long userId, byte[] newHash, byte[] newSalt);
    }
}
