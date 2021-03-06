using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Interfaces
{
    public interface IUnitOfWork
    {
         IUserRepository userRepository { get; }
         IMessageRepository messageRepository { get; }
         ILikesRepository LikesRepository { get; }
         IPhotoRepository PhotoRepository { get; }
        Task<bool> Complete();
        bool HasChanges();
    }
}