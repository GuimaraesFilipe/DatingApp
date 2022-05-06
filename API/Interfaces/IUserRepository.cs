using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Controllers.DTOs;
using API.Entities;

namespace API.Interfaces
{
    public interface IUserRepository
    {
        void Update(AppUser user);
        Task<bool> SaveAllAsync();
        Task<IEnumerable<AppUser>>GetUsersAsync();
        Task<AppUser>GetUserByIdAsync (int id);
        Task<AppUser>GetUserByUsernameAsync(string username);

        Task<IEnumerable<MemberDto>> getMembersAsync();

        Task<MemberDto> GetMemberAsync(string username);
        
    }
}