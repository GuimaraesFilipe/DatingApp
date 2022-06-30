using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Controllers.DTOs;
using API.Entities;

namespace API.Interfaces
{
    public interface IPhotoRepository
    {
        Task<Photo>GetPhotoById (int id);
      Task<IEnumerable<PhotoForApproval>> GetUnapprovedPhotos();
        void removePhoto(Photo photo);
    }
}