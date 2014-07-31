using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using PIMS.Core.Models;


namespace PIMS.IntegrationTest.Security
{
    [TestFixture]
    public class VerifyAuthorization
    {

        private const string UrlBase = "http://localhost/PIMS.Web.Api/Api/Account";
        private LoginModel _login;
        //private UserManager<ApplicationUser> _userMgr;



        // TODO:  why doesn't this read the correct conn string from web.config, instead of machine.config ?
        //private readonly string _connString = ConfigurationManager.ConnectionStrings["PIMS-ConnString"].ConnectionString;
        private const string ConnString = @"Data Source=RICHARD-VAIO\RICHARDDB;Initial Catalog='Lighthouse - PIMS - Test';Integrated Security=True";



        [SetUp]
        public void Init()
        {
           // _lastAccessToken = string.Empty;

            // Min length for password = 6.
            _login = new LoginModel
                            {
                                UserName = "TestUser0731a",
                                Password = "pwrd0731a"
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
        public async void Middleware_returns_an_accessToken_via_a_successful_login()
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


        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Middleware_will_not_allow_use_of_an_expired_token_on_the_service()
        {
            using (var client = new HttpClient())
            {
                // Arrange
                // a. Create token, with expiration time, via login on an existing authenticated user.
                // b. Try to access WebApi, via just created token, using configured expired token time of 5 sec.
                //    [ * Reset AccessTokenExpireTimeSpan for other tests or WebApi use * ]

                // a:
                client.BaseAddress = new Uri(UrlBase);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var settings = new JsonSerializerSettings();
                var ser = JsonSerializer.Create(settings);
                var j = JObject.FromObject(_login, ser);
                HttpContent content = new StringContent(j.ToString());
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var loginResponse = await client.PostAsync(client.BaseAddress + "/LoginAsync", content);
                var accessToken = loginResponse.Content.ReadAsStringAsync().Result;
               

                // Act
                // b:
                Thread.Sleep(TimeSpan.FromSeconds(6));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
                var authorizationResponse = await client.GetAsync(client.BaseAddress + "/SecurityTestMethod"); 


                // Assert
                Assert.IsNotNullOrEmpty(accessToken);
                Assert.IsTrue(loginResponse.Content.Headers.ContentLength >= 500);
                Assert.IsTrue(authorizationResponse.StatusCode == HttpStatusCode.Unauthorized);
            }

        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Middleware_will_allow_use_of_a_token_on_the_service_after_a_successful_login()
        {
            using (var client = new HttpClient())
            {
                // After a successful login, we'll use the issued access token to gain access to the web service. The SAME UNEXPIRED
                // token will be used for each subsequent web service interaction, as would be the case where the client
                // would be supplying the granted token in a real use case scenario. If the token has expired, a new login will be required.
                // Running this test multiple times on same user with an unexpired token, will result in an "unauthorized" (401) access error.
                // [* Expiration is based on : AccessTokenExpireTimeSpan configuration in StartUpAuth *]

                // Arrange
                client.BaseAddress = new Uri(UrlBase);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var settings = new JsonSerializerSettings();
                var ser = JsonSerializer.Create(settings);
                var j = JObject.FromObject(_login, ser);
                HttpContent content = new StringContent(j.ToString());
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var loginResponse = await client.PostAsync(client.BaseAddress + "/LoginAsync", content);
                var accessToken = loginResponse.Content.ReadAsStringAsync().Result;
                // Reintialize returned token w/o default escape sequences.
                accessToken = accessToken.Substring(1, accessToken.Length -2 ); 


                // Act
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var testMethodResponse = await client.GetAsync(client.BaseAddress + "/SecurityTestMethod");
                var responseContent = await testMethodResponse.Content.ReadAsStringAsync();

                // Assert
                Assert.IsNotNullOrEmpty(accessToken);
                Assert.IsTrue(loginResponse.Content.Headers.ContentLength >= 500);
                Assert.IsTrue(testMethodResponse.StatusCode == HttpStatusCode.OK);
                Assert.IsTrue(responseContent.IndexOf("granted", System.StringComparison.Ordinal) > 1);
            }

        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Middleware_will_not_allow_invalid_username_login() 
        {
            using (var client = new HttpClient()) 
            {
                // Arrange
                _login = new LoginModel
                            {
                                UserName = "TestUs0731a",
                                Password = "pwrd0731a"
                            };

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

                // Assert
                Assert.IsTrue(loginResponse.StatusCode == HttpStatusCode.InternalServerError);
            }

        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Middleware_will_not_allow_invalid_password_login() {
            using (var client = new HttpClient()) {
                // Arrange
                _login = new LoginModel {
                    UserName = "TestUser0731a",
                    Password = "pwr 0731a"
                };

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

                // Assert
                Assert.IsTrue(loginResponse.StatusCode == HttpStatusCode.InternalServerError);
            }

        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Middleware_will_not_return_an_accessToken_via_an_unsuccessful_login()
        {
            using (var client = new HttpClient())
            {
                // Arrange
                _login = new LoginModel {
                    UserName = "TestUser0731a",
                    Password = "prd0731a"
                };

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
                Assert.IsTrue(loginResponse.StatusCode == HttpStatusCode.InternalServerError);
                Assert.IsTrue(accessToken.IndexOf("exceptionMessage", System.StringComparison.Ordinal) >= 1);
            }

        }





    }
}
