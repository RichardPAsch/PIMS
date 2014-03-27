using System;
using System.Collections.Generic;

namespace PIMS.Core.Models
{
    public class User 
    {
        public virtual  Guid UserId { get; set; }

        public virtual  string LastName { get; set; }

        public virtual  string FirstName { get; set; }

        public virtual  string Password { get; set; }

        public virtual  string UserName { get; set; }

        public virtual  string EMail { get; set; }

        public virtual IList<Asset> Security { get; set; }

        public virtual List<Link> Links { get; set; }


        //public virtual  byte[] Version { get; set; }

        // Omit Asset collection; creates circular reference: Asset <--> User
        //private readonly IList<Asset> _assets = new List<Asset>();
        //public virtual IList<Asset> Assets { get { return _assets; } } 
    }
}
