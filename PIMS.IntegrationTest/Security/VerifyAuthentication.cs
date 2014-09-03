using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NHibernate.AspNet.Identity;
using NUnit.Framework;
using PIMS.Core.Models;
using PIMS.Web.Api;


namespace PIMS.IntegrationTest.Security
{
    [TestFixture]
    public class VerifyAuthentication
    {
        //private AccountController _ctrl;
        private const string UrlBase = "http://localhost/PIMS.Web.API/api/Account";
        private RegistrationModel _registration;
        private UserManager<ApplicationUser> _userMgr; // needed ?
    


        // TODO:  why doesn't this read the correct conn string from web.config, instead of machine.config ?
        //private readonly string _connString = ConfigurationManager.ConnectionStrings["PIMS-ConnString"].ConnectionString;
        private const string ConnString = @"Data Source=RICHARD-VAIO\RICHARDDB;Initial Catalog='Lighthouse - PIMS - Test';Integrated Security=True";



        [SetUp]
        public void Init()
        {
            // Min length for password = 6.
            // Ex: UserName = "TestUser0828a" // Password = "pwrd0828a" // ConfirmPassword = "pwrd0828a"
            _registration = new RegistrationModel
                            {
                                UserName = "TestUser0903a",
                                Password = "pwrd0903a",
                                ConfirmPassword = "pwrd0903a"
                            };

            _userMgr = new UserManager<ApplicationUser>(
                new NHibernate.AspNet.Identity.UserStore<ApplicationUser>(NHibernateConfiguration.CreateSessionFactory(ConnString).OpenSession()));
        }




        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Controller_Can_POST_a_new_Registerd_User()
        {
            using (var client = new HttpClient())
            {
                // Arrange
                client.BaseAddress = new Uri(UrlBase);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
           

                // Manually set "content-type" header to reflect serialized data format.
                var settings = new JsonSerializerSettings();
                var ser = JsonSerializer.Create(settings);
                var j = JObject.FromObject(_registration, ser);
                HttpContent content = new StringContent(j.ToString());
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                

                // Act
                // PostAsJsonAsync(), per MS recommendation, however results in problem of RegisterAsync() accepting payload: content type?
                // Serialized registration data is now associated with HttpContent element explicitly, with format then set.
                var response = await client.PostAsync(client.BaseAddress + "/RegisterAsync", content); 
                //var response = await client.PostAsJsonAsync(client.BaseAddress + "/RegisterAsync", _registration); // error
                
                // Assert
                Assert.IsTrue(response.StatusCode == HttpStatusCode.Created);
                Assert.IsTrue(response.IsSuccessStatusCode);
                
            }
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Controller_Can_not_POST_a_duplicate_Registerd_User() {

            // Duplicate user = same user name.
            using (var client = new HttpClient()) {

                // Arrange
                client.BaseAddress = new Uri(UrlBase);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Ensure use of an existing user, independent of other tests.
                _registration = new RegistrationModel {
                    UserName = "RPATest0711c",
                    Password = "pwrd11c",
                    ConfirmPassword = "pwrd11c"
                };

                var settings = new JsonSerializerSettings();
                var ser = JsonSerializer.Create(settings);
                var j = JObject.FromObject(_registration, ser);
                HttpContent content = new StringContent(j.ToString());
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");


                // Act
                var response = await client.PostAsync(client.BaseAddress + "/RegisterAsync", content);

                // Assert
                Assert.IsTrue(response.StatusCode == HttpStatusCode.BadRequest);
                Assert.IsTrue(response.ReasonPhrase.Contains("already taken"));

            }
        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Controller_Can_not_POST_a_new_Registerd_User_with_an_invalid_name() {

            // Valid user name = letters or digits only.
            using (var client = new HttpClient()) {

                // Arrange
                client.BaseAddress = new Uri(UrlBase);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Ensure use of an existing user, independent of other tests.
                _registration = new RegistrationModel {
                    UserName = "RPA$_",
                    Password = "pwrd11c",
                    ConfirmPassword = "pwrd11c"
                };

                var settings = new JsonSerializerSettings();
                var ser = JsonSerializer.Create(settings);
                var j = JObject.FromObject(_registration, ser);
                HttpContent content = new StringContent(j.ToString());
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");


                // Act
                var response = await client.PostAsync(client.BaseAddress + "/RegisterAsync", content);

                // Assert
                Assert.IsTrue(response.StatusCode == HttpStatusCode.BadRequest);
                Assert.IsTrue(response.ReasonPhrase.Contains("letters or digits"));

            }
        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Controller_Can_not_POST_a_new_Registerd_User_with_any_invalid_data() {

            /* ** Invalid data: **
             * non-matching passwords
             * missing password and/or password confirmation
             * password length < 6
             * non-alpha numeric chars
            */

            using (var client = new HttpClient()) {

                // Arrange
                client.BaseAddress = new Uri(UrlBase);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Ensure use of an existing user, independent of other tests.
                _registration = new RegistrationModel {
                    UserName = "RPA$_",
                    Password = "pwrd11c",
                    ConfirmPassword = "pwrd11c"
                };

                var settings = new JsonSerializerSettings();
                var ser = JsonSerializer.Create(settings);
                var j = JObject.FromObject(_registration, ser);
                HttpContent content = new StringContent(j.ToString());
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");


                // Act
                var response = await client.PostAsync(client.BaseAddress + "/RegisterAsync", content);

                // Assert
                Assert.IsTrue(response.StatusCode == HttpStatusCode.BadRequest);
                Assert.IsTrue(response.ReasonPhrase.Contains("letters or digits"));

            }
        }


       



    }
}

