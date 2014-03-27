using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;

namespace PIMS.Web.Common.Security
{
    // created 9/26/13 -RPA
    public interface IMembershipAdapter
    {
        MembershipUserWrapper GetUser(string username);

        MembershipUserWrapper GetUser(Guid userId);

        MembershipUserWrapper CreateMembershipUserWrapper(MembershipUser user);
 
        MembershipUserWrapper CreateUser(string username, string password, string email);

        bool ValidateUser(string username, string password);

        string[] GetRolesForUser(string username) ;
    }

}
