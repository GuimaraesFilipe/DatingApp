using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers.DTOs
{
    public class PhotoForApproval
    { public int Id { get; set; }
        public string Url { get; set; }
        public string UserName { get; set; }
          public bool IsApproved { get; set; }
        
    }
}