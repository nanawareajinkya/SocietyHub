using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocietyHub.Application.Common.Features.Users.DTOs
{
    public record UserRegisterDto(
       string Username,
       string Password,
       string? Email,
       string? Phone,
       string? RoleCode
   );
}
