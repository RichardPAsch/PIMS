using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PIMS.Web.Api.ViewModel
{
    // SERVICE model type; ViewModel for client.
    public class User
    {
        public Guid UserId { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Password { get; set; }
        public string UserName { get; set; }
        public string EMail { get; set; }
        public List<Link> Links { get; set; }
    }
}