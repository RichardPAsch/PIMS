using System;
using System.Collections.Generic;
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
        public UserManager<ApplicationUser> UserManager { get; private set; }
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
            UserManager = userManager;
        }
        
        //TODO: ctors needed, if so, when?
        //public AccountController(ISessionFactory sf) {
        //    if (sf == null) throw new ArgumentNullException("sf");
        //    _sf = sf;
        //}

        //// Default parameterless ctor.
        //public AccountController() : this(new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(NHibernateConfiguration.CheckSession(_sf)))) {
        //}

    

       



        private static IAuthenticationManager AuthenticationManager
        {
           // you can call this method as an instance method on any object of type HttpRequestMessage, per
            // http://msdn.microsoft.com/en-us/library/system.net.http.owinhttprequestmessageextensions.getowincontext(v=vs.118).aspx

             get { return  HttpContext.Current.GetOwinContext().Authentication; } 
        }


        
        private async Task SignInAsync(ApplicationUser user, bool isPersistent) {
            try
            {
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                var identity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
                AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistent }, identity);
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                throw;
            }

            // Needed ??
           // AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
           // Enable the application to use a cookie to store information for the signed in user
            //var identity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            //AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistent }, identity);
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
            var identityResult = await UserManager.CreateAsync(user, registrationData.Password);

            HttpResponseMessage responseMsg = null;
            var errorMsg = string.Empty;
            if (identityResult.Succeeded) {
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
            if (!ModelState.IsValid) return "Invalid login or unregistered.";

            using (var client = new HttpClient())
            {
                var login = new Dictionary<string, string>
                           {
                               {"grant_type", "password"},
                               {"username", loginData.UserName},
                               {"password", loginData.Password}
                           };

                var response = await client.PostAsync(UrlBase + "/token", new FormUrlEncodedContent(login));
                var content = await response.Content.ReadAsStringAsync();

                var json = JObject.Parse(content);
                return json["access_token"].ToString();
            }

           


            //var user = await UserManager.FindAsync(model.UserName, model.Password);
            //if (user != null) {
            //    await SignInAsync(user, model.RememberMe);
            //    return Ok();
            //}
            //ModelState.AddModelError("", "Invalid username and/or password.");

            // If we got this far, something failed, redisplay form
            //return StatusCode(HttpStatusCode.Unauthorized);
        }



        [System.Web.Http.HttpGet]
        [System.Web.Http.Authorize]
        public IHttpActionResult SecurityTestMethod()
        {
            return Ok("Access granted.");

        }




        //private async Task SignInAsync(ApplicationUser user, bool isPersistent) {
        //    AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
        //    var identity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
        //    AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, identity);
        //}


        //private void AddErrors(IdentityResult result) {
        //    foreach (var error in result.Errors) {
        //        ModelState.AddModelError("", error);
        //    }
        //}

        //// POST: /Account/LogOff
        //[ValidateAntiForgeryToken]
        //public IHttpActionResult LogOff() {
        //    AuthenticationManager.SignOut();
        //    return Ok();
        //}


        //private bool HasPassword() {
        //    var user = UserManager.FindById(User.Identity.GetUserId());
        //    if (user != null) {
        //        return user.PasswordHash != null;
        //    }
        //    return false;
        //}


        ////
        //// GET: /Account/Manage
        //public string Manage(ManageMessageId? message) {
        //    var statusMessage =
        //              message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
        //            : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
        //            : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
        //            : message == ManageMessageId.Error ? "An error has occurred."
        //            : "";

        //    var hasPwrd = HasPassword();
        //    //ViewBag.ReturnUrl = Url.Action("Manage");
        //    return statusMessage;
        //}



    }

    


}
