using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Controllers.DTOs;

using API.Data;
using API.Extensions;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Helpers;

namespace API.Controllers
{
   [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly DataContext _context;
        public readonly IMapper _mapper ;
        public readonly IPhotoService _photoService;
        private readonly IUnitOfWork _unitOfWork;
        public UsersController(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService)
        {
            _unitOfWork = unitOfWork;
            _photoService = photoService;
            _mapper = mapper;
            
            
        }
        [Authorize]
        [HttpGet]
       
        public async Task< ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
        {
            var gender = await _unitOfWork.userRepository.GetUserGender(User.GetUsername());
            userParams.CurrentUserName = User.GetUsername();
            if (string.IsNullOrEmpty(userParams.Gender))
                userParams.Gender = gender == "female" ? "male" : "female";

            var users = await _unitOfWork.userRepository.getMembersAsync(userParams);
            
            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);
            return Ok(users);

         
        }

        //api/users/id
        [Authorize]
        [HttpGet("{username}", Name = "GetUser")]
        public async Task< ActionResult<MemberDto> > GetUser(string  username)
        {

           var currentUsername = User.GetUsername();
            return await _unitOfWork.userRepository.GetMemberAsync(username,
            isCurrentUser: currentUsername == username
            );



        }
        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDTO memberUpdateDTO){

            var user = await _unitOfWork.userRepository.GetUserByUsernameAsync(User.GetUsername());

            _mapper.Map(memberUpdateDTO, user);

            _unitOfWork.userRepository.Update(user);

            if (await _unitOfWork.Complete()) return NoContent();

            return BadRequest("Failed to Update user");
        }
        
        [HttpPost("add-photo")]

        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
           var user= await _unitOfWork.userRepository.GetUserByUsernameAsync(User.GetUsername());
           var result = await _photoService.AddPhotoAsync(file);

           if (result.Error != null) return BadRequest(result.Error.Message);

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };
            user.Photos.Add(photo);

           if (await _unitOfWork.Complete())
           {
                return CreatedAtRoute("GetUser", new {username = user.UserName},_mapper.Map<PhotoDto>(photo) );

           }
          

           return BadRequest("Problem adding photo");


        }
        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId){

            var user= await _unitOfWork.userRepository.GetUserByUsernameAsync(User.GetUsername());

            var photo = user.Photos.FirstOrDefault(x => x.id ==photoId);

            if(photo.IsMain) return BadRequest("this is already your main photo");

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
            if (currentMain != null) currentMain.IsMain= false;
            photo.IsMain=true;

            if (await _unitOfWork.Complete()) return NoContent();

            return BadRequest("failed to set main photo");
        }


        [HttpDelete ("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId){
                var user= await _unitOfWork.userRepository.GetUserByUsernameAsync(User.GetUsername());
        var photo = user.Photos.FirstOrDefault(x => x.id ==photoId);

        if(photo == null) return NotFound();

        if (photo.IsMain) return BadRequest("You cannot delete your main photo");

        if(photo.PublicId != null)
        {
            var result= await _photoService.DeletePhotoAsync(photo.PublicId);
            if(result.Error != null) return BadRequest(result.Error.Message);

        }
        user.Photos.Remove(photo);
        if(await _unitOfWork.Complete()) return Ok();
        return BadRequest("failed to delete photo");
        }


    }
}