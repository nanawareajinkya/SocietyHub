using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocietyHub.Application.Common.Features.Users.DTOs
{
    public record UserDto(
        long UserId,
        string Username,
        string? Email,
        string? Phone,
        bool IsActive
    )
    {
        public string? Token { get; set; }
    };
}
