using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Microsoft.Owin.Security;
using NHibernate;
using PIMS.Core.Models;
using Microsoft.AspNet.Identity;


namespace PIMS.Web.Api.Controllers
{
    public class AccountController : ApiController
    {

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

           // AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
           // Enable the application to use a cookie to store information for the signed in user
            //var identity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            //AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistent }, identity);
        }



        // POST: [.../Account/RegisterAsync]
        [System.Web.Http.HttpPost]
        [System.Web.Http.AllowAnonymous]
        [ValidateAntiForgeryToken]
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


            


        

        //// POST: /Account/Login
        //[System.Web.Http.HttpPost]
        //[System.Web.Http.AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<IHttpActionResult> Login(LoginViewModel model, string returnUrl) {
        //    if (!ModelState.IsValid) return Unauthorized();
        //    var user = await UserManager.FindAsync(model.UserName, model.Password);
        //    if (user != null) {
        //        await SignInAsync(user, model.RememberMe);
        //        return Ok();
        //    }
        //    ModelState.AddModelError("", "Invalid username and/or password.");

        //    // If we got this far, something failed, redisplay form
        //    return StatusCode(HttpStatusCode.Unauthorized);
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
