using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Mvc;
using Microsoft.Owin.Security;
using Newtonsoft.Json.Linq;
using NHibernate;
using PIMS.Core.Models;
using Microsoft.AspNet.Identity;
using PIMS.Core.Models.ViewModels;


namespace PIMS.Web.Api.Controllers
{
    [System.Web.Http.RoutePrefix("api/Account")]
    [EnableCorsAttribute("http://localhost:5969", "*", "*")]
    public class AccountController : ApiController
    {
        // TODO: Replace hard-coded path for 'localhost'
        private const string UrlBase = "http://localhost/PIMS.Web.Api";
        private static ISessionFactory _sf = null;


        public UserManager<ApplicationUser> UserMgr { get; private set; }
        public object SignInStub { get; set; }
        


        # region ctors

            public AccountController(UserManager<ApplicationUser> userManager) {
                UserMgr = userManager;
            }


            public AccountController() : this(new UserManager<ApplicationUser>(new NHibernate.AspNet.Identity.UserStore<ApplicationUser>(NHibernateConfiguration.CheckSession(_sf)))) {
            }
        
        #endregion

        
        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            Error
        }
        

        private static IAuthenticationManager AuthenticationManager
        {
           // you can call this method as an instance method on any object of type HttpRequestMessage, per
           // http://msdn.microsoft.com/en-us/library/system.net.http.owinhttprequestmessageextensions.getowincontext(v=vs.118).aspx

             get { return  HttpContext.Current.GetOwinContext().Authentication; } 
        }
        

        public async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalBearer); // added 8-3-15
            //AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var claimsIdentity = await UserMgr.CreateIdentityAsync(user, DefaultAuthenticationTypes.ExternalBearer);  // added 8-3-15
            //var claimsIdentity = await UserMgr.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistent }, claimsIdentity);
        }
        

        // POST: [.../Account/RegisterAsync]
        [System.Web.Http.HttpPost]
        [System.Web.Http.AllowAnonymous]
        [ValidateAntiForgeryToken]
        // Include "action" in route template to avoid Http 500: "Multiple actions were found that match the request".
        //[System.Web.Http.Route("RegisterAsync")]
        [System.Web.Http.Route("")]
        public async Task<IHttpActionResult> RegisterAsync([FromBody] RegistrationModel registrationData) {

            if (!ModelState.IsValid) return BadRequest("Invalid registration data received.");
            var user = new ApplicationUser { UserName = registrationData.UserName };
            var identityResult = await UserMgr.CreateAsync(user, registrationData.Password);   // data persisted
            //CurrentIdentity = identityResult;

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
        public async Task<IHttpActionResult> LoginAsync([FromBody] LoginModel loginData)
        {
            // Creates token upon successful login.
            if (!ModelState.IsValid) return BadRequest("Invalid login or unregistered");

            // Registration data via 'AspNetUsers' table.
            var user = await UserMgr.FindAsync(loginData.UserName, loginData.Password);
            if (user == null)
                return BadRequest("No registration data found, or error accessing info.");

            await SignInAsync(user, loginData.RememberMe);
            using (var client = new HttpClient())
            {
                var login = new Dictionary<string, string>
                        {
                            {"grant_type", "password"},
                            {"username", loginData.UserName},
                            {"password", loginData.Password}
                        };

                // Access token endpoint on authorization server. UrlBase -> http://localhost/PIMS.Web.Api
                var response = await client.PostAsync(UrlBase + "/token", new FormUrlEncodedContent(login));
                if (response == null)
                    return BadRequest("Unable to create access token for: " + loginData.UserName.Trim());

                var content = await response.Content.ReadAsStringAsync();

                var jsonResponse = JObject.Parse(content);

                // TODO: Also return user name as confirmation of successful login ? returnUrl = /Account/LoginAsync ?
                //return json["access_token"].ToString() + "[Welcome " + loginData.UserName + "]";
                //return Ok(json["access_token"].ToString());
                return Ok(jsonResponse);
            }  

        }



        [System.Web.Http.HttpGet]
        [System.Web.Http.Authorize]
        public IHttpActionResult SecurityTestMethod()
        {
            return Ok("Access granted.");

        }



        // POST: /Account/ChangePasswordAsync
        [System.Web.Http.HttpPost]
        [ValidateAntiForgeryToken]
        [System.Web.Http.Route("ChangePasswordAsync")]
        public async Task<IHttpActionResult> ChangePasswordAsync([FromBody] ChangePasswordVm editedData)
        {
            if (!ModelState.IsValid || (editedData.ConfirmPassword.Trim() != editedData.NewPassword.Trim())) return BadRequest("Invalid password edits.");

            var currentInvestor = new Core.Security.PimsIdentityService();
            var appUser = UserMgr.FindByNameAsync(currentInvestor.CurrentUser);

            if (string.IsNullOrEmpty(appUser.Result.Id))
                return ResponseMessage(new HttpResponseMessage
                                    {
                                        StatusCode = HttpStatusCode.BadRequest,
                                        ReasonPhrase = "Invalid user"
                                    });

            var identityResult = await UserMgr.ChangePasswordAsync(appUser.Result.Id, editedData.OldPassword, editedData.NewPassword);
           
            return ResponseMessage(identityResult.Succeeded ?
                    new HttpResponseMessage { StatusCode = HttpStatusCode.OK, ReasonPhrase = "Password sucessfully updated for investor:  " + currentInvestor.CurrentUser } :
                    new HttpResponseMessage { StatusCode = HttpStatusCode.NotModified, ReasonPhrase = "Unable to update password, due to: " + identityResult.Errors.First().Trim() });

        }


        // POST api/Account/Logout
        [System.Web.Http.Route("Logout")]
        public IHttpActionResult Logout()
        {
            // TODO: Verify via UI testing ?
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalBearer);
            return Ok();
        }


        


        #region Helpers

            //private IHttpActionResult GetErrorResult(IdentityResult result)
            //{
            //    if (result == null) {
            //        return InternalServerError();
            //    }

            //    if (result.Succeeded) return null;
            //    if (result.Errors != null)
            //    {
            //        foreach (var error in result.Errors)
            //        {
            //            ModelState.AddModelError("", error);
            //        }
            //    }

            //    // No ModelState errors available; returning empty BadRequest.
            //    if (ModelState.IsValid) {
            //        return BadRequest();
            //    }

            //    return BadRequest(ModelState);
            //}


        #endregion


        #region D.Kurata referenced (APM.WebApi) TODO functionality via AccountController

        /*
        // POST api/Account/Logout
        [Route("Logout")]   // Implemented 8-13-15
        public IHttpActionResult Logout() {
            Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            return Ok();
        }
         
         // POST api/Account/ChangePassword
        [Route("ChangePassword")]   // Implemented 8-14-15
        public async Task<IHttpActionResult> ChangePassword(ChangePasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword,
                model.NewPassword);
            
            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }
        
        
        // POST api/Account/RemoveLogin
        [Route("RemoveLogin")]
        public async Task<IHttpActionResult> RemoveLogin(RemoveLoginBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result;

            if (model.LoginProvider == LocalLoginProvider)
            {
                result = await UserManager.RemovePasswordAsync(User.Identity.GetUserId());
            }
            else
            {
                result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(),
                    new UserLoginInfo(model.LoginProvider, model.ProviderKey));
            }

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }
        
        
        
         
         */

        #endregion

    }

    


}
