using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using PIMS.Core.Models;


namespace PIMS.IntegrationTest.Security
{
    [TestFixture]
    public class VerifyAuthorization
    {
        //private AccountController _ctrl;
        private const string UrlBase = "http://localhost/PIMS.Web.Api/Api/Account";
        private LoginModel _login;
        //private UserManager<ApplicationUser> _userMgr;



        // TODO:  why doesn't this read the correct conn string from web.config, instead of machine.config ?
        //private readonly string _connString = ConfigurationManager.ConnectionStrings["PIMS-ConnString"].ConnectionString;
        private const string ConnString = @"Data Source=RICHARD-VAIO\RICHARDDB;Initial Catalog='Lighthouse - PIMS - Test';Integrated Security=True";



        [SetUp]
        public void Init()
        {
            // Min length for password = 6.
            _login = new LoginModel
                            {
                                UserName = "TestUser0723c",
                                Password = "pwrd0723c"
                            };
        }




        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Middleware_can_sucessfully_grant_a_token()
        {
            using (var client = new HttpClient())
            {
                // Arrange - using login credentials of authenticated user.
                // ** Exceeding AccessTokenExpireTimeSpan since creation, will result in "invalid_grant" server error. **
                var post = new Dictionary<string, string>
                           {
                               {"grant_type", "password"},
                               {"username", "TestUser072214a"},
                               {"password", "pwrd0722a"}
                           };

                // Act
                var response = await client.PostAsync(UrlBase + "/token", new FormUrlEncodedContent(post));
                var content = await response.Content.ReadAsStringAsync();


                // Assert
                Assert.IsNotNullOrEmpty(content);
                Assert.IsTrue(content.IndexOf("bearer", System.StringComparison.Ordinal) > 0);

            }
        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Middleware_returns_an_accessToken_via_a_successfull_login()
        {
            using (var client = new HttpClient())
            {
                // Arrange
                // Prerequisite: login data to be used, MUST derive from successfull user registration creation 
                //               based on AccessTokenExpireTimeSpan in StartUp.cs.
                client.BaseAddress = new Uri(UrlBase);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var settings = new JsonSerializerSettings();
                var ser = JsonSerializer.Create(settings);
                var j = JObject.FromObject(_login, ser);
                HttpContent content = new StringContent(j.ToString());
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");


                // Act
                var loginResponse = await client.PostAsync(client.BaseAddress + "/LoginAsync", content);
                var accessToken = loginResponse.Content.ReadAsStringAsync().Result;


                // Assert
                Assert.IsTrue(loginResponse.StatusCode == HttpStatusCode.OK);
                Assert.IsNotNullOrEmpty(accessToken);
                Assert.IsTrue(loginResponse.Content.Headers.ContentLength >= 500 );

            }

        }

    }
}
