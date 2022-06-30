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
using API.Interfaces;
using API.Services;

namespace API.Controllers
{
    public class AdminController: BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        public readonly IPhotoService _photoService;

        public AdminController(UserManager<AppUser> userManager,IUnitOfWork
unitOfWork, IPhotoService photoService)
        {
            
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _photoService = photoService;
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

        public async Task<ActionResult> GetPhotosForModeration()
            {
            var photos = await
            _unitOfWork.PhotoRepository.GetUnapprovedPhotos();
            return Ok(photos);
            }

           [Authorize(Policy ="ModeratorPhotoRole")]
            [HttpPost("approve-photo/{photoId}")]
            public async Task<ActionResult> ApprovePhoto(int photoId)
            {
           var photo = await
            _unitOfWork.PhotoRepository.GetPhotoById(photoId);
            if (photo == null) return NotFound("Could not find photo");
            photo.IsApproved = true;
            var user = await
            _unitOfWork.userRepository.GetUserByPhotoId(photoId);
            if (!user.Photos.Any(x => x.IsMain)) photo.IsMain = true;
            await _unitOfWork.Complete();
                        return Ok();
                }

            [Authorize(Policy = "ModeratorPhotoRole")]
            [HttpPost("reject-photo/{photoId}")]
            public async Task<ActionResult> RejectPhoto(int photoId)
            {
                var photo = await _unitOfWork.PhotoRepository.GetPhotoById(photoId);
                if (photo.PublicId != null)
                {
                   var result= await _photoService.DeletePhotoAsync(photo.PublicId);
                     if(result.Error != null) return BadRequest(result.Error.Message);
                    if (result.Result == "ok")
                    {
                        _unitOfWork.PhotoRepository.removePhoto(photo);
                    }
                }
                else
                {
                    _unitOfWork.PhotoRepository.removePhoto(photo);
                }
                await _unitOfWork.Complete();
                return Ok();
            }

    }
}