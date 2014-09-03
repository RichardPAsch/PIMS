using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PIMS.Core.Models
{
 
        // TODO: This will need to be re-evaluated, considering already existing User entity.
        public class Account
        {
            public virtual Guid Id { get; set; }

            public virtual string AccountName { get; set; }

            public virtual string AccountPassword { get; set; }

            public virtual string AccountPasswordConfirm { get; set; }
        }
   
}
