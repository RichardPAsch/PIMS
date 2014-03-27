using System;

namespace PIMS.Web.Common.Security
{
    public class MembershipUserWrapper
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
    }
}