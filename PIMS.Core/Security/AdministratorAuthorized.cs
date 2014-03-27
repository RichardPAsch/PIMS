using System.Web.Http;

namespace PIMS.Core.Security

{
    public class AdministratorAuthorized : AuthorizeAttribute
    {
        public AdministratorAuthorized()
        {
            Roles = "Administrators";
        }
    }
}