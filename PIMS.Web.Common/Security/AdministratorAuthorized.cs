using System.Web.Http;
using System.Web.Mvc;


namespace PIMS.Web.Common.Security

{
    public class AdministratorAuthorized : AuthorizeAttribute
    {
        public AdministratorAuthorized()
        {
            Roles = "Administrators";
        }
    }
}