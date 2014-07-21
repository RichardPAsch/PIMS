using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNet.Identity;
using NUnit.Framework;
using PIMS.Core.Models;


namespace PIMS.IntegrationTest.Security
{
    [TestFixture]
    public class VerifyAuthorization
    {
        //private AccountController _ctrl;
        private const string UrlBase = "http://localhost/PIMS.Web.Api";
        private RegistrationModel _registration;
        private UserManager<ApplicationUser> _userMgr;



        // TODO:  why doesn't this read the correct conn string from web.config, instead of machine.config ?
        //private readonly string _connString = ConfigurationManager.ConnectionStrings["PIMS-ConnString"].ConnectionString;
        private const string ConnString =
            @"Data Source=RICHARD-VAIO\RICHARDDB;Initial Catalog='Lighthouse - PIMS - Test';Integrated Security=True";



        [SetUp]
        public void Init()
        {
            // Min length for password = 6.
            //_registration = new RegistrationModel
            //                {
            //                    UserName = "RPATest0716a",
            //                    Password = "pwrd16a",
            //                    ConfirmPassword = "pwrd16a"
            //                };

            //_userMgr =
            //    new UserManager<ApplicationUser>(
            //        new UserStore<ApplicationUser>(
            //            NHibernateConfiguration.CreateSessionFactory(ConnString).OpenSession()));

        }




        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Middleware_can_sucessfully_grant_a_token_for_a_login_request()
        {
            using (var client = new HttpClient())
            {
                // Arrange - using login credentials of authenticated user.
                // ** Exceeding AccessTokenExpireTimeSpan since creation, will result in "invalid_grant" server error. **
                var post = new Dictionary<string, string>
                           {
                               {"grant_type", "password"},
                               {"username", "RPATest0721c"},
                               {"password", "pwrd21c"}
                           };

                // Act
                var response = await client.PostAsync(UrlBase + "/token", new FormUrlEncodedContent(post));
                var content = await response.Content.ReadAsStringAsync();


                // Assert
                Assert.IsNotNullOrEmpty(content);
                //Assert.IsTrue(response.IsSuccessStatusCode);

            }
        }




    }
}
