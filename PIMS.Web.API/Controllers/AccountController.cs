using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Microsoft.Owin.Security;
using Newtonsoft.Json.Linq;
using NHibernate;
using PIMS.Core.Models;
using Microsoft.AspNet.Identity;


namespace PIMS.Web.Api.Controllers
{
    [System.Web.Http.RoutePrefix("api/Account")]
    public class AccountController : ApiController
    {
        private const string UrlBase = "http://localhost/PIMS.Web.Api";
        public UserManager<ApplicationUser> UserMgr { get; private set; }
        public object SignInStub { get; set; }
        private static ISessionFactory _sf;

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            Error
        }


        // ctors
        public AccountController(UserManager<ApplicationUser> userManager)
        {
            UserMgr = userManager;
        }
        
        // Default
        public AccountController()
            : this(new UserManager<ApplicationUser>(new NHibernate.AspNet.Identity.UserStore<ApplicationUser>(NHibernateConfiguration.CheckSession(_sf)))) {
        }

       
        


        private static IAuthenticationManager AuthenticationManager
        {
           // you can call this method as an instance method on any object of type HttpRequestMessage, per
           // http://msdn.microsoft.com/en-us/library/system.net.http.owinhttprequestmessageextensions.getowincontext(v=vs.118).aspx

             get { return  HttpContext.Current.GetOwinContext().Authentication; } 
        }
        

        public async Task SignInAsync(ApplicationUser user, bool isPersistent) {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var claimsIdentity = await UserMgr.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistent }, claimsIdentity);
        }
        

        // POST: [.../Account/RegisterAsync]
        [System.Web.Http.HttpPost]
        [System.Web.Http.AllowAnonymous]
        [ValidateAntiForgeryToken]
        // Include "action" in route template to avoid Http 500: "Multiple actions were found that match the request".
        [System.Web.Http.Route("RegisterAsync")]
        public async Task<IHttpActionResult> RegisterAsync([FromBody] RegistrationModel registrationData) {

            if (!ModelState.IsValid) return BadRequest("Invalid registration data received.");
            var user = new ApplicationUser { UserName = registrationData.UserName };
            var identityResult = await UserMgr.CreateAsync(user, registrationData.Password);

            var errorMsg = string.Empty;
            if (identityResult.Succeeded)
            {
                await SignInAsync(user, false); 
            }
            else
            {
                var enumerator = identityResult.Errors.GetEnumerator();
                if (enumerator.MoveNext())
                {
                    errorMsg = enumerator.Current;
                }

                return ResponseMessage(new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest, ReasonPhrase = errorMsg });
            }

            return ResponseMessage(new HttpResponseMessage { StatusCode = HttpStatusCode.Created });
        }



        // POST: /Account/LoginAsync
        [System.Web.Http.HttpPost]
        [System.Web.Http.AllowAnonymous]
        [ValidateAntiForgeryToken]
        [System.Web.Http.Route("LoginAsync")]
        public async Task<string> LoginAsync([FromBody] LoginModel loginData)
        {
            // Creates token upon successful login.
            if (!ModelState.IsValid) return "Invalid login or unregistered.";

            var user = await UserMgr.FindAsync(loginData.UserName, loginData.Password);
            if (user != null)
            {
                await SignInAsync(user, loginData.RememberMe);
                using (var client = new HttpClient())
                {
                    var login = new Dictionary<string, string>
                           {
                               {"grant_type", "password"},
                               {"username", loginData.UserName},
                               {"password", loginData.Password}
                           };

                    // Access token endpoint on authorization server.
                    var response = await client.PostAsync(UrlBase + "/token", new FormUrlEncodedContent(login));
                    var content = await response.Content.ReadAsStringAsync();

                    var json = JObject.Parse(content);
                    // TODO: Also return user name as confirmation of successful login ? returnUrl = /Account/LoginAsync ?
                    //return json["access_token"].ToString() + "[Welcome " + loginData.UserName + "]";
                    return json["access_token"].ToString();
                }  
            }

            return "Unable to create access token.";
        }



        [System.Web.Http.HttpGet]
        [System.Web.Http.Authorize]
        public IHttpActionResult SecurityTestMethod()
        {
            return Ok("Access granted.");

        }



        // POST: /Account/Manage
        [System.Web.Http.HttpPost]
        [ValidateAntiForgeryToken]
        [System.Web.Http.Route("ManageAsync")]
        public async Task<IHttpActionResult> ManageAsync([FromBody] ManageUserModel manageData)
        {
            if (!ModelState.IsValid || (manageData.ConfirmPassword.Trim() != manageData.NewPassword.Trim())) return BadRequest("Invalid password edits.");

            var errMsg = string.Empty;
            var identityResult = new IdentityResult();
            var currUserId = User.Identity.GetUserId();

            if (string.IsNullOrEmpty(currUserId))
                return ResponseMessage(new HttpResponseMessage
                                    {
                                        StatusCode = HttpStatusCode.BadRequest,
                                        ReasonPhrase = "Invalid user"
                                    });
            try
            {
                identityResult = await UserMgr.ChangePasswordAsync(currUserId, manageData.OldPassword, manageData.NewPassword);
            }
            catch (Exception ex) {
                errMsg = ex.Message.ToString(CultureInfo.InvariantCulture); //debug
            }

            return ResponseMessage(identityResult.Succeeded ?
                    new HttpResponseMessage { StatusCode = HttpStatusCode.OK, ReasonPhrase = "Password updated for user:  " + currUserId } :
                    new HttpResponseMessage { StatusCode = HttpStatusCode.NotModified, ReasonPhrase = "Unable to update password, due to: " + errMsg });

        }



    }

    


}
