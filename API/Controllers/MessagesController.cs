using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Controllers.DTOs;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API.Extensions;
using API.Entities;
using AutoMapper;
using API.Helpers;

namespace API.Controllers
{
    [Authorize]
    public class MessagesController : BaseApiController
    {
     
        public readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public MessagesController(IUnitOfWork unitOfWork, IMapper mapper){
            _unitOfWork = unitOfWork;
            _mapper = mapper;
          
            
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>>CreateMessage(CreateMessageDto createMessageDto)
        {
            var username = User.GetUsername();

            if (username ==createMessageDto.RecipientUserName.ToLower())
                return BadRequest("You cannot send messages to yourself");

            var sender = await _unitOfWork.userRepository.GetUserByUsernameAsync(username);
            var recipient=await _unitOfWork.userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUserName);

            if (recipient==null) return NotFound();

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUserName = recipient.UserName,
                Content = createMessageDto.Content
            };
            _unitOfWork.messageRepository.AddMessage(message);

            if (await _unitOfWork.Complete()) return Ok(_mapper.Map<MessageDto>(message));

            return BadRequest("failed to send message");


        }

        [HttpGet]

        public async Task <ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams){
            messageParams.Username = User.GetUsername();

            var messages = await _unitOfWork.messageRepository.GetMessagesForUser(messageParams);
            Response.AddPaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages);
            return messages;
        }
        
          [HttpDelete("{id}")]

          public async Task<ActionResult> DeleteMessage(int id){
            var username = User.GetUsername();
            var message = await _unitOfWork.messageRepository.GetMessage(id);
            if (message.Sender.UserName != username && message.Recipient.UserName != username)
                return Unauthorized();

            if ( message.Sender.UserName== username) message.SenderDeleted = true;
                
            if ( message.Recipient.UserName== username) message.RecipientDeleted = true;
            
            if ( message.SenderDeleted && message.RecipientDeleted)
                _unitOfWork.messageRepository.DeleteMessage(message);

            if(await _unitOfWork.Complete()) return Ok();
            return BadRequest("problem deleting the message");
        }
    }
}