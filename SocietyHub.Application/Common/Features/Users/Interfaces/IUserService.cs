using SocietyHub.Application.Common.Features.Users.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocietyHub.Application.Common.Features.Users.Interfaces
{
    public interface IUserService
    {
        Task<long> RegisterAsync(UserRegisterDto dto);
        Task<UserDto?> LoginAsync(UserLoginRequestDto dto, string clientIp, string userAgent);
        Task<UserDto?> GetByIdAsync(long id);
        Task UpdateAsync(long id, string? email, string? phone);
        Task ChangePasswordAsync(long id, string currentPassword, string newPassword);
    }
}
