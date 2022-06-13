using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using API.Entities;
using API.Controllers;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AdminController: BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;

        public AdminController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        [Authorize(Policy ="RequireAdminRole")]
        [HttpGet("users-with-roles")]

        public async Task<ActionResult> GetUserWithRoles(){

            var users= await _userManager.Users
            .Include(r =>r.UserRoles)
            .ThenInclude(r =>r.Role)
            .OrderBy(u => u.UserName)
            .Select( u => new 
            {
                u.Id,
                username=u.UserName,
                Roles= u.UserRoles.Select(r => r.Role.Name).ToList()
            })
            .ToListAsync();

            return Ok(users);
        }

        [HttpPost ("edit-roles/{username}")]
        public async Task<ActionResult> EditRoles (string username, [FromQuery] string roles){

            var selectedRoles = roles.Split(",").ToArray();

            var user = await _userManager.FindByNameAsync(username);
             if (user== null) return NotFound("Could not find the user");
            var UserRoles = await _userManager.GetRolesAsync(user);

            var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(UserRoles));

            if (!result.Succeeded) return BadRequest("Failed to addd to roles");

            result = await _userManager.RemoveFromRolesAsync(user, UserRoles.Except(selectedRoles));

            if (!result.Succeeded) return BadRequest("Failed to remove from roles");

            return Ok(await _userManager.GetRolesAsync(user));
        }

         [Authorize(Policy ="ModeratorPhotoRole")]
        [HttpGet("photos-to-moderate")]

        public ActionResult GetPhotosForModeration(){

            return Ok("Admins or Moderators can see these photos");
        }
        
    }
}